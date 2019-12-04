using Microsoft.AspNetCore.Mvc;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner;
using Newtonsoft.Json;
using ent.manager.WebApi.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using ent.manager.Services.Subscription;
using ent.manager.Services.User;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.Services.Product;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using ent.manager.Services.Reporting.Report;
using ent.manager.Entity.Model.Reporting;
using System.Collections.Generic;
using ent.manager.Reporting.Model;
using ent.manager.Services.DeviceTypeDictionary;
using ent.manager.Services.DeviceModelDictionary;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ent.manager.Services.SubscriptionAuth;
using ent.manager.Services.Reporting.ReportProcessorRun;
using ent.manager.Reporting;
using System.Threading.Tasks;
using static ent.manager.Entity.Model.wEnum;
using ent.manager.Entity.Model;
using System.Net;

namespace ent.manager.WebApi.Controllers
{
    public class ReportApiController : Controller
    {

        private ISubscriptionService _subscriptionService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private IReportService _reportService;
        private IUsageReportService _usageReportService;
        private IDeviceOSReportService _deviceOSReportService;
        private ISeatDetailsReportService _seatDetailsReportService;
        private IUserDataService _userDataService;
        private IDeviceManufacturerReportService _deviceManufacturerReportService;
        private IDeviceTypeReportService _deviceTypeReportService;
        private IProductService _productService;
        private ILogger<ReportApiController> _logger { get; set; }
        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        private IDeviceTypeDictionaryService _deviceTypeDictionaryService;
        private IDeviceModelDictionaryService _deviceModelDictionaryService;
        private IMemoryCache _cache;
        private static IConfigurationRoot _configuration;
        private ISubscriptionAuthService _subscriptionAuthService;
        private IReportProcessorRunService _reportProcessorRunService;
        private ILogger<ReportProvider> _reportLogger { get; set; }
        private IReportProcessor _reportProcessor;
        private IUserService _userService;

        public ReportApiController(ISubscriptionService subscriptionService,
            ILicenceEnvironmentService licenceEnvironmentService,
            IReportService reportService,
            IUsageReportService usageReportService,
            IDeviceOSReportService deviceOSReportService,
            ISeatDetailsReportService seatDetailsReportService,
            IUserDataService userDataService,
            IDeviceManufacturerReportService deviceManufacturerReportService,
            IDeviceTypeReportService deviceTypeReportService,
             IProductService productService,
             ILogger<ReportApiController> logger,
             IEnterpriseClientService enterpriseClientService,
             IPartnerService partnerService,
             IDeviceTypeDictionaryService deviceTypeDictionaryService,
             IDeviceModelDictionaryService deviceModelDictionaryService,
             IMemoryCache cache,
             ISubscriptionAuthService subscriptionAuthService,
             IReportProcessorRunService reportProcessorRunService,
             IReportProcessor reportProcessor,
             ILogger<ReportProvider> reportLogger,
             IUserService userService)
        {
            _subscriptionService = subscriptionService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _reportService = reportService;
            _usageReportService = usageReportService;
            _deviceOSReportService = deviceOSReportService;
            _seatDetailsReportService = seatDetailsReportService;
            _userDataService = userDataService;
            _deviceManufacturerReportService = deviceManufacturerReportService;
            _deviceTypeReportService = deviceTypeReportService;
            _productService = productService;
            _logger = logger;
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
            _deviceTypeDictionaryService = deviceTypeDictionaryService;
            _deviceModelDictionaryService = deviceModelDictionaryService;
            _cache = cache;
            _subscriptionAuthService = subscriptionAuthService;
            _reportProcessorRunService = reportProcessorRunService;
            _reportProcessor = reportProcessor;
            _reportLogger = reportLogger;
            _configuration = CommonHelper.GetConfigurationObject();
            _userService = userService;
            FillCache();
        }

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult Fetch(int s)
        {
            try
            {
                var subscription = _subscriptionService.GetById(s);

                if (subscription == null)
                    return Json(new
                    {
                        c = ResultCode.ReportResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });

                //Check Authorization status 
                if (!IsAuthorised(subscription.LicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                Report report;


                report = _reportService.GetLatestBySubId(subscription.Id);

                if (report == null)
                    return Json(new
                    {
                        c = ResultCode.ReportResultCodes.ReportDoesntExist,
                        d = ""
                    });


                var usageReportReportRecord = _usageReportService.GetByReportId(report.Id);
                var deviceManufacturerReportRecord = _deviceManufacturerReportService.GetByReportId(report.Id);
                var deviceOsReportRecord = _deviceOSReportService.GetByReportId(report.Id);
                var deviceTypeReportRecord = _deviceTypeReportService.GetByReportId(report.Id);
                var seatDetailsReportRecord = _seatDetailsReportService.GetByReportId(report.Id).Skip(10*3).Take(10);


                //SubscriptionDetails Fills

                var productName = "";
                var enterpriseName = "";
                var partnerName = "";

                var product = _productService.GetByWId(subscription.Product);

                if (product != null)
                {
                    productName = product.Name;
                }

                var enterprise = _enterpriseClientService.GetById(subscription.EnterpriseClientId);

                if (enterprise != null)
                {
                    enterpriseName = enterprise.Name;

                    var partner = _partnerService.GetById(enterprise.PartnerId);

                    if (partner != null) partnerName = partner.Name;
                }


                return Json(new
                {
                    c = ResultCode.Success,
                    d = new
                    {
                        reportdate = report.CompletionTime,
                        reportid = report.Id,
                        subscriptiondetails = new {name = subscription.Name, enterpriseapplication = productName, seats = subscription.SeatCount, enterprisename = enterpriseName, managingpartner = partnerName, subdate = subscription.CreationTime, lk = subscription.LicenceKey, clientDownloadLocation = subscription.ClientDownloadLocation,
                            SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                            SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
                        },
                        usagereport = usageReportReportRecord == null ? new { report = false, available = -1, used = -1 } : new { report = true, available = usageReportReportRecord.Available, used = usageReportReportRecord.Used},
                        devicemanufacturerreport = deviceManufacturerReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceManufacturerReportRecord.Data) },
                        deviceosreport = deviceOsReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceOsReportRecord.Data) },
                        devicetypereport = deviceTypeReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceTypeReportRecord.Data) },
                        seatdetailsreport = seatDetailsReportRecord == null ? new { report = false, data = new List<SeatDetailsReport>() } : new { report = true, data = GetSeatDetailsReportRecordTranslated(seatDetailsReportRecord.ToList()) }

                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_fetch"));
                
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        class SeatDetailsReportFilters
        {
            public string fn { get; set; }
            public string ln { get; set; }
            public string od { get; set; }
            public string dn { get; set; }

        }

     
        [HttpPost]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult FetchPaged([FromBody]dynamic value)
        {
            try
            {

                IEnumerable<SeatDetailsReport> seatDetailsReportRecord = new List<SeatDetailsReport>();

                var pageSize = int.Parse(_configuration["PageSize"]);

                string svalue = Convert.ToString(value);

                dynamic listQuery = JsonConvert.DeserializeObject(svalue);

                int subscriptionId = Convert.ToInt32(listQuery["s"].Value);

                int pageIndex = Convert.ToInt32(listQuery["i"].Value);

                //var filtersJson = listQuery["f"];

                //SeatDetailsReportFilters filters =  JsonConvert.DeserializeObject<SeatDetailsReportFilters>(filtersJson.ToString());

                var subscription = _subscriptionService.GetById(subscriptionId);

                if (subscription == null)
                    return Json(new
                    {
                        c = ResultCode.ReportResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });

                //Check Authorization status 
                if (!IsAuthorised(subscription.LicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                Report report;


                report = _reportService.GetLatestBySubId(subscription.Id);

                if (report == null)
                    return Json(new
                    {
                        c = ResultCode.ReportResultCodes.ReportDoesntExist,
                        d = ""
                    });


                var usageReportReportRecord = _usageReportService.GetByReportId(report.Id);
                var deviceManufacturerReportRecord = _deviceManufacturerReportService.GetByReportId(report.Id);
                var deviceOsReportRecord = _deviceOSReportService.GetByReportId(report.Id);
                var deviceTypeReportRecord = _deviceTypeReportService.GetByReportId(report.Id);
                //var seatDetailsReportRecordAll = _seatDetailsReportService.GetByReportIdPaged(report.Id, filters.fn, filters.ln, filters.od, filters.dn);
                //var total = seatDetailsReportRecordAll.Count();
                //var pagecount = (total / pageSize) + 1;

                //if (pageIndex == 1)
                //{
                //     seatDetailsReportRecord = seatDetailsReportRecordAll.Take(pageSize);

                //}
                //else
                //{
                //     seatDetailsReportRecord = seatDetailsReportRecordAll.Skip(pageSize * (pageIndex - 1) ).Take(pageSize);
                //}
    


                //SubscriptionDetails Fills

                var productName = "";
                var enterpriseName = "";
                var partnerName = "";

                var product = _productService.GetByWId(subscription.Product);

                if (product != null)
                {
                    productName = product.Name;
                }

                var enterprise = _enterpriseClientService.GetById(subscription.EnterpriseClientId);

                if (enterprise != null)
                {
                    enterpriseName = enterprise.Name;

                    var partner = _partnerService.GetById(enterprise.PartnerId);

                    if (partner != null) partnerName = partner.Name;
                }


                return Json(new
                {
                    c = ResultCode.Success,
                    d = new
                    {
                        reportdate = report.CompletionTime,
                        reportid = report.Id,
                        subscriptiondetails = new { name = subscription.Name, enterpriseapplication = productName, seats = subscription.SeatCount, enterprisename = enterpriseName, managingpartner = partnerName, subdate = subscription.CreationTime, lk=subscription.LicenceKey, clientDownloadLocation = subscription.ClientDownloadLocation,
                            SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                            SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
                        },
                        usagereport = usageReportReportRecord == null ? new { report = false, available = -1, used = -1 } : new { report = true, available = usageReportReportRecord.Available, used = usageReportReportRecord.Used },
                        devicemanufacturerreport = deviceManufacturerReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceManufacturerReportRecord.Data) },
                        deviceosreport = deviceOsReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceOsReportRecord.Data) },
                        devicetypereport = deviceTypeReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceTypeReportRecord.Data) }
                        //,
                        //seatdetailsreport = seatDetailsReportRecord == null ? new { report = false, i = 0, t = 0, p = 0 , data = new List<SeatDetailsReport>() } : new { report = true, i = pageIndex, t = total, p = pagecount, data = GetSeatDetailsReportRecordTranslated(seatDetailsReportRecord.ToList()) }

                    }
                   
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_fetch"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }


        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult FetchByDate(int s, string d)
        {
            try
            {
                var subscription = _subscriptionService.GetById(s);

                if (subscription == null)
                    return Json(new
                    {
                        c = ResultCode.ReportResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });

                //Check Authorization status 
                if (!IsAuthorised(subscription.LicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                Report report;

                var reportTime = ParseFetchDate(d);

                if (reportTime.Year == 1)
                {
                    return Json(new
                    {
                        c = ResultCode.ReportResultCodes.InvalidDateFilter,
                        d = ""
                    });
                }
                else
                {
                    report = _reportService.GetLatestByDateBySubId(subscription.Id, reportTime);

                    if (report == null)
                        return Json(new
                        {
                            c = ResultCode.ReportResultCodes.ReportDoesntExist,
                            d = ""
                        });
                }

                var usageReportReportRecord = _usageReportService.GetByReportId(report.Id);
                var deviceManufacturerReportRecord = _deviceManufacturerReportService.GetByReportId(report.Id);
                var deviceOsReportRecord = _deviceOSReportService.GetByReportId(report.Id);
                var deviceTypeReportRecord = _deviceTypeReportService.GetByReportId(report.Id);
                var seatDetailsReportRecord = _seatDetailsReportService.GetByReportId(report.Id);


                //SubscriptionDetails Fills

                var productName = "";
                var enterpriseName = "";
                var partnerName = "";

                var product = _productService.GetByWId(subscription.Product);

                if (product != null)
                {
                    productName = product.Name;
                }

                var enterprise = _enterpriseClientService.GetById(subscription.EnterpriseClientId);

                if (enterprise != null)
                {
                    enterpriseName = enterprise.Name;

                    var partner = _partnerService.GetById(enterprise.PartnerId);

                    if (partner != null) partnerName = partner.Name;
                }


                return Json(new
                {
                    c = ResultCode.Success,
                    d = new
                    {
                        reportdate = report.CompletionTime,
                        reportid = report.Id,
                        subscriptiondetails = new { name = subscription.Name, enterpriseapplication = productName, seats = subscription.SeatCount, enterprisename = enterpriseName, managingpartner = partnerName, subdate = subscription.CreationTime, lk = subscription.LicenceKey, clientDownloadLocation = subscription.ClientDownloadLocation,
                            SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                            SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
                        },
                        usagereport = usageReportReportRecord == null ? new { report = false, available = -1, used = -1 } : new { report = true, available = usageReportReportRecord.Available, used = usageReportReportRecord.Used },
                        devicemanufacturerreport = deviceManufacturerReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceManufacturerReportRecord.Data) },
                        deviceosreport = deviceOsReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceOsReportRecord.Data) },
                        devicetypereport = deviceTypeReportRecord == null ? new { report = false, data = new List<ReportGroupItemModel>() } : new { report = true, data = GetGroupedDataTopSixObjectsTranslated(deviceTypeReportRecord.Data) },
                        seatdetailsreport = seatDetailsReportRecord == null ? new { report = false, data = new List<SeatDetailsReport>() } : new { report = true, data = GetSeatDetailsReportRecordTranslated(seatDetailsReportRecord.ToList()) }

                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_fetchbydate"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public  async Task  TriggerReportService()
        {
            try
            {

                ReportProvider rp = new ReportProvider(_reportProcessorRunService, 
                    _reportLogger,
                    _subscriptionService,
                    _reportProcessor, 
                    true);

                await rp.CallReportProcessor(new System.Threading.CancellationToken());
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_start"));
            }



        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetReportProcessorRuns()
        {
            //TimeZoneInfo localZone = TimeZoneInfo.FindSystemTimeZoneById(_configuration["timezones:destinationtimezoneid"]);
         

            try
            {
                var prList = _reportProcessorRunService.GetAll();

                var query = from pr in prList
                            select new
                            {
                                Id = pr.Id,
                                ReportsCount = pr.ReportsCount,
                                StartTimeUTC = pr.StartRunTime.ToString("dd/MM/yyyy HH:mm:ss"),
                                EndTimeUTC = pr.EndRunTime.HasValue ? pr.EndRunTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                                //StartTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(pr.StartRunTime, localZone).ToString("dd/MM/yyyy HH:mm:ss"),
                                //EndTimeLocal = pr.EndRunTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(pr.StartRunTime, localZone).ToString("dd/MM/yyyy HH:mm:ss") : "",
                                Status = ((ReportProcessorRunStatus)pr.Status).convertToString()
                            };

                return Json(new
                {
                    c = ResultCode.Success,
                    d = query.OrderByDescending(x => x.Id).ToList()
                });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_getprocessorruns"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }


        private DateTime ParseFetchDate(string value)
        {

            int year = 1;
            int month = 1;
            int day = 1;

            try
            {
                day = int.Parse(value.Substring(0, 2));
                month = int.Parse(value.Substring(2, 2));
                year = int.Parse(value.Substring(4, 4));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_ParseFetchDate"));
                return new DateTime(1, 1, 1);
            }

            return new DateTime(year: year, day: day, month: month);

        }

        private List<ReportGroupItemModel> GetGroupedDataTopSixObjects(string value)
        {

            try
            {
                var obj = JsonConvert.DeserializeObject<List<ReportGroupItemModel>>(value);

                var resultData = obj;

                if (resultData.Count > 6)
                {
                    resultData.OrderByDescending(x => x.count).Take(6).ToList();
                    return resultData;
                }
                else
                {
                    return resultData;

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_GetGroupedDataTopSixObjects"));
                return new List<ReportGroupItemModel>();

            }


        }

        private List<ReportGroupItemModel> GetGroupedDataTopSixObjectsTranslated(string value)
        {

            try
            {
                List <ReportGroupItemModel> obj = JsonConvert.DeserializeObject<List<ReportGroupItemModel>>(value);

                List <ReportGroupItemModel> resultData = obj;

                List<ReportGroupItemModel> resultDataOutPut = new List<ReportGroupItemModel>();
                //Translate

                foreach (var item in resultData)
                {
                    string translation = "";

                    if(_cache.TryGetValue(item.name, out translation))
                    {
                        resultDataOutPut.Add(new ReportGroupItemModel() {  name = translation, count = item.count}) ;
                    }
                    else
                    {
                        resultDataOutPut.Add(new ReportGroupItemModel() { name = item.name, count = item.count });
                    }
                }
                

                if (resultDataOutPut.Count > 6)
                {
                    resultDataOutPut.OrderByDescending(x => x.count).Take(6).ToList();
                    return resultDataOutPut;
                }
                else
                {
                    return resultDataOutPut;

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_GetGroupedDataTopSixObjectsTranslated"));
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<ReportGroupItemModel>>(value);

                    var resultData = obj;

                    if (resultData.Count > 6)
                    {
                        resultData.OrderByDescending(x => x.count).Take(6).ToList();
                        return resultData;
                    }
                    else
                    {
                        return resultData;

                    }

                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2.GetLogText("reportapi_exception_GetGroupedDataTopSixObjects"));
                    return new List<ReportGroupItemModel>();

                }


            }


        }

        private List<SeatDetailsReport> GetSeatDetailsReportRecordTranslated(List<SeatDetailsReport> list)
        {

            try
            {
                //Translate
                //var result  = list.Select(c => { c.DeviceModel = GetValueFromCache(c.DeviceModel); return c; }).ToList();
                
                list.ForEach(c => {
                    c.DeviceModel = GetValueFromCache(c.DeviceModel);
                    c.DeviceType = GetValueFromCache(c.DeviceType);
                });

                return list;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("reportapi_GetSeatDetailsReportRecordTranslated"));
                return list;
            }

        }

        private string GetValueFromCache(string value)
        {

            string translation = "";

            if (_cache.TryGetValue(value, out translation))
            {
                return translation;
            }
            else
            {
                return value;
            }

        }

        private void FillCache()
        {
            try
            {
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                          // Keep in cache for this time, reset time if accessed.
                          .SetSlidingExpiration(TimeSpan.FromMinutes(int.Parse(_configuration["CacheExpiryMinutes"])));

                //our isfilled entry to let us know if our cache is filled 
                var isfilled = "";

                //if (((MemoryCache)_cache).Count <= 5)
                if (!_cache.TryGetValue("isfilled", out isfilled))
                {  
                    _cache.Set("isfilled", "isfilled", cacheEntryOptions);

                    var devicemodeldictionarylist = _deviceModelDictionaryService.GetAll();

                    var devicetypedictionarylist = _deviceTypeDictionaryService.GetAll();
           
                    string translation = "";

                    foreach (var item in devicemodeldictionarylist)
                    {
                        if (!_cache.TryGetValue(item.Name, out translation))
                        {

                          

                            // Save data in cache.
                            _cache.Set(item.Name, item.wName, cacheEntryOptions);
                        }
                    }

                    foreach (var item in devicetypedictionarylist)
                    {
                        if (!_cache.TryGetValue(item.Name, out translation))
                        {

                        

                            // Save data in cache.
                            _cache.Set(item.Name, item.wName, cacheEntryOptions);
                        }
                    }
                }
             
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("reportapi_FillCache"));
            }
          
        }

        private bool IsAuthorised(string licenceKey)
        {
            try
            {
                var result = false;
                var TokenDetails = User.Claims.GetTokenDetails();


                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    return result;
                }


                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                if (managerTokenUser == null && TokenDetails.Role.ToLower() == "admin")
                {
                    return true;
                }

                var subscription = _subscriptionService.GetByLicenceKey(licenceKey);

                if (subscription == null) return result;

                var sub_ec = _enterpriseClientService.GetById(subscription.EnterpriseClientId);

                var sub_partner = _partnerService.GetById(sub_ec.PartnerId);

                if (TokenDetails.Role.ToLower() == "admin")
                {
                    result = true;

                }
                else if (TokenDetails.Role.ToLower() == "partner")
                {
                    if (sub_partner.Id == managerTokenUser.PartnerId)
                    {
                        result = true;
                    }

                }
                else if (TokenDetails.Role.ToLower() == "ec")
                {
                    if ((sub_partner.Id == managerTokenUser.PartnerId) && (sub_ec.Id == managerTokenUser.EnterpriseId))
                    {
                        result = true;
                    }

                }

                return result;

            }
            catch
            {

                return false;
            }

        }

    }
}
