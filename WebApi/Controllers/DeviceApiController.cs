using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Authorization;
using ent.manager.Services.Subscription;
using ent.manager.Entity.Model;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.Licence.Model;
using System.Threading.Tasks;
using ent.manager.Services.Product;
using Microsoft.Extensions.Logging;
using ent.manager.WebApi.Helpers;
using static ent.manager.Entity.Model.wEnum;
using ent.manager.WebApi.Results;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using ent.manager.Services.User;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner;
using System.Net;

namespace ent.manager.WebApi.Controllers
{
    public class DeviceApiController : Controller
    {

        private ISubscriptionService _subscriptionService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private IProductService _productService;
        private ILogger<LicenceApiController> _logger { get; set; }
        private IConfigurationRoot _configuration;
        private IUserService _userService;
        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public DeviceApiController(ISubscriptionService subscriptionService,
            ILicenceEnvironmentService licenceEnvironmentService,
            IProductService productService, IUserService userService, IEnterpriseClientService enterpriseClientService, IPartnerService partnerService, ILogger<LicenceApiController> logger)
        {
            _subscriptionService = subscriptionService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _productService = productService;
            _logger = logger;
            _userService = userService;
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
            _configuration = CommonHelper.GetConfigurationObject();
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> DeviceInformationEventsAsync([FromBody]DeviceInformationEventsFilter filter)
        {
            try
            {

                //Result
                var returnResult = new DeviceInformationResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);


                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_iOS || productType == ProductType.SC_SS_Android || productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                {
                    _event = "01";
                    _subevent = "01";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latestMatchedEvent = result.d.OrderByDescending(x=>x.EventDate).ToArray()[0];

                                    if (productType == ProductType.SC_SS_iOS)
                                    {
                                        var IOSDeviceInformation = JsonConvert.DeserializeObject<IOSDeviceInformation[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.deviceInformation = IOSDeviceInformation[0];
                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_Android)
                                    {
                                        var AndroidDeviceInformation = JsonConvert.DeserializeObject<AndroidDeviceInformation[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.deviceInformation = AndroidDeviceInformation[0];
                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        var WinDeviceInformation = JsonConvert.DeserializeObject<WinDeviceInformation[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.deviceInformation = GetWinDeviceInformation(WinDeviceInformation[0]);
                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_MacOs)
                                    {
                                        var MacDeviceInformation = JsonConvert.DeserializeObject<MacDeviceInformation[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.deviceInformation = MacDeviceInformation[0];
                                        return Json(returnResult);
                                    }


                                    //return JsonConvert.SerializeObject(result.d);
                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_DeviceInformationEventsAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_DeviceInformationEventsAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        private WinDeviceInformation GetWinDeviceInformation(WinDeviceInformation winDeviceInformation)
        {
            winDeviceInformation.DisplayMemory = ConvertMemoryFormat(winDeviceInformation.Memory);

            return winDeviceInformation;

        }

        private string ConvertMemoryFormat(string memorySize)
        {
            var size = memorySize.ConvertToInt() / 1000;

            return size > 1 ? $"{size}GB" : $"{memorySize}MB";
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> WebProtectEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {


                //Result
                var returnResult = new WebProtectResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_iOS || productType == ProductType.SC_SS_Android)
                {
                    _event = "03";
                    _subevent = "03";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latestMatchedEvent = result.d.OrderByDescending(x=>x.EventDate).ToArray()[0];

                                    if (productType == ProductType.SC_SS_iOS || productType == ProductType.SC_SS_Android)
                                    {
                                        var webProtectStatus = JsonConvert.DeserializeObject<WebProtectStatus[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.webProtectStatus = webProtectStatus[0];
                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        returnResult.result = -3;
                        _logger.LogError(ex.GetLogText("devapi_WebProtectEventAsync"));
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_WebProtectEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> DeviceHealthEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {
                //Result
                var returnResult = new DeviceHealthResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_iOS || productType == ProductType.SC_SS_Android || productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                {
                    _event = "05";
                    _subevent = "01";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    
                                    var latestMatchedEvent = result.d.OrderByDescending(x=>x.EventDate).ToArray()[0];

                                    if (productType == ProductType.SC_SS_iOS || productType == ProductType.SC_SS_Android || productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                                    {
                                        var DeviceHealth = JsonConvert.DeserializeObject<DeviceHealth[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.deviceHealth = DeviceHealth[0];
                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_DeviceHealthEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_DeviceHealthEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> ScanSummaryEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {
                //Result
                var returnResult = new ScanSummaryResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_iOS || productType == ProductType.SC_SS_Android || productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                {
                    _event = "02";
                    _subevent = "01";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latest10MatchedEvents = result.d.OfType<Event>().OrderByDescending(x=>x.EventDate).ToList().Take(10);

                                    //try
                                    //{
                                    //    latest10MatchedEvents = latest10MatchedEvents.OrderByDescending(x => DateTime.Parse(x.EventDate)).ToList();

                                    //}
                                    //catch
                                    //{ }


                                    if (productType == ProductType.SC_SS_iOS)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var scanSummaryResult = JsonConvert.DeserializeObject<IOSScanSummaryResult[]>(item.Description);
                                            returnResult.scanSummaryResult.Add(scanSummaryResult[0]);
                                        }

                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }
                                    if (productType == ProductType.SC_SS_Android)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var scanSummaryResult = JsonConvert.DeserializeObject<AndroidScanSummaryResult[]>(item.Description);
                                            returnResult.scanSummaryResult.Add(scanSummaryResult[0]);
                                        }
                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }
                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var scanSummaryResult = JsonConvert.DeserializeObject<WinScanSummaryResult[]>(item.Description);
                                            returnResult.scanSummaryResult.Add(scanSummaryResult[0]);
                                        }
                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }
                                    if (productType == ProductType.SC_SS_MacOs)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var scanSummaryResult = JsonConvert.DeserializeObject<MacScanSummaryResult[]>(item.Description);
                                            returnResult.scanSummaryResult.Add(FormatMacScanSummaryResult(scanSummaryResult[0]));
                                        }
                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_ScanSummaryEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_ScanSummaryEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        private MacScanSummaryResult FormatMacScanSummaryResult(MacScanSummaryResult macScanSummaryResult)
        {
            macScanSummaryResult.ScanDate = macScanSummaryResult.ScanStartDateTime.ConvertToDate("yyyy-MM-dd hh:mm:ss").ToString("dd/MM/yyyy");
            macScanSummaryResult.ScanDurationMinutes = GetDurationByMinutes(macScanSummaryResult.ScanDuration);

            return macScanSummaryResult;

        }

        private string GetDurationByMinutes(string duration)
        {
            int result;
            var parts = duration.Split(':');
            if (parts.Length != 3)
                result = 1;

            var seconds = parts[0].ConvertToInt() * 3600 + parts[1].ConvertToInt() * 60 + parts[2].ConvertToInt();

            result = Convert.ToInt32(Math.Ceiling(Decimal.Divide(seconds, 60)));

            return result <= 1 ? $"{result} minute" : $"{result} minutes";
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> MalwareDetectionEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {


                //Result
                var returnResult = new MalwareDetectionEventResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_Android || productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                {
                    _event = "02";
                    _subevent = "02";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latest10MatchedEvents = result.d.OfType<Event>().OrderByDescending(x=>x.EventDate).ToList().Take(10);

                                    //try
                                    //{
                                    //    latest10MatchedEvents = latest10MatchedEvents.OrderByDescending(x => DateTime.Parse(x.EventDate)).ToList();

                                    //}
                                    //catch
                                    //{ }

                                    if (productType == ProductType.SC_SS_Android)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var malwareDetectionEvent = JsonConvert.DeserializeObject<AndroidMalwareDetectionEvent[]>(item.Description);
                                            var malwareDetectEvent = malwareDetectionEvent[0];
                                            malwareDetectEvent.eventDate = item.EventDate;
                                            returnResult.malwareDetectionEvents.Add(malwareDetectionEvent[0]);
                                        }

                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var malwareDetectionEvent = JsonConvert.DeserializeObject<WinMalwareDetectionEvent[]>(item.Description);
                                            var malwareDetectEvent = malwareDetectionEvent[0];
                                            malwareDetectEvent.eventDate = item.EventDate;
                                            returnResult.malwareDetectionEvents.Add(malwareDetectionEvent[0]);
                                        }

                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_MacOs)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var malwareDetectionEvent = JsonConvert.DeserializeObject<MacMalwareDetectionEvent[]>(item.Description);
                                            var malwareDetectEvent = malwareDetectionEvent[0];
                                            malwareDetectEvent.eventDate = item.EventDate;
                                            returnResult.malwareDetectionEvents.Add(malwareDetectionEvent[0]);
                                        }

                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_MalwareDetectionEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_MalwareDetectionEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> SecureAppsEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {

                //Result
                var returnResult = new SecureAppsEventResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_Android)
                {
                    _event = "06";
                    _subevent = "01";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latest10MatchedEvents = result.d.OfType<Event>().OrderByDescending(x=>x.EventDate).ToList().Take(10);

                                    //try
                                    //{
                                    //    latest10MatchedEvents = latest10MatchedEvents.OrderByDescending(x => DateTime.Parse(x.EventDate)).ToList();

                                    //}
                                    //catch
                                    //{ }


                                    if (productType == ProductType.SC_SS_Android)
                                    {


                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var secureAppsEventResult = JsonConvert.DeserializeObject<SecureAppsEvent[]>(item.Description);
                                            var secureAppEvent = secureAppsEventResult[0];
                                            secureAppEvent.eventDate = item.EventDate;
                                            returnResult.secureAppsEvents.Add(secureAppEvent);
                                        }

                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_SecureAppsEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_SecureAppsEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> MalwareRemediationEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {
                //Result
                var returnResult = new MalwareRemediationEventResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                {
                    _event = "02";
                    _subevent = "03";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latest10MatchedEvents = result.d.OfType<Event>().OrderByDescending(x=>x.EventDate).ToList().Take(10);

                                    //try
                                    //{
                                    //    latest10MatchedEvents = latest10MatchedEvents.OrderByDescending(x => DateTime.Parse(x.EventDate)).ToList();

                                    //}
                                    //catch
                                    //{ }


                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var malwareRemediationEventResult = JsonConvert.DeserializeObject<WinMalwareRemediationEventResult[]>(item.Description);
                                            returnResult.malwareRemediationEventResult.Add(malwareRemediationEventResult[0]);
                                        }
                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_MacOs)
                                    {
                                        foreach (var item in latest10MatchedEvents)
                                        {
                                            var malwareRemediationEventResult = JsonConvert.DeserializeObject<MacMalwareRemediationEventResult[]>(item.Description);
                                            returnResult.malwareRemediationEventResult.Add(malwareRemediationEventResult[0]);
                                        }
                                        returnResult.result = 1;

                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_MalwareRemediationEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_MalwareRemediationEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> RealtimeProtectionEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {


                //Result
                var returnResult = new RealTimeProtectionEventResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_Win || productType == ProductType.SC_SS_MacOs)
                {
                    _event = "03";
                    _subevent = "01";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latestMatchedEvent = result.d.OrderByDescending(x=>x.EventDate).ToArray()[0];

                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        var realTimeProtectionStatus = JsonConvert.DeserializeObject<RealTimeProtectionEvent[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.realTimeProtectionEvent = realTimeProtectionStatus[0];
                                        return Json(returnResult);
                                    }

                                    if (productType == ProductType.SC_SS_MacOs)
                                    {
                                        var realTimeProtectionStatus = JsonConvert.DeserializeObject<RealTimeProtectionEvent[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.realTimeProtectionEvent = realTimeProtectionStatus[0];
                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_RealtimeProtectionEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_RealtimeProtectionEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> FirewallEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {


                //Result
                var returnResult = new FirewallEventResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_Win)
                {
                    _event = "03";
                    _subevent = "02";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latestMatchedEvent = result.d[0];

                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        var firewallEvent = JsonConvert.DeserializeObject<FirewallEvent[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.firewallEvent = firewallEvent[0];
                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_FirewallEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_FirewallEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public virtual async System.Threading.Tasks.Task<IActionResult> FirewallPolicyEventAsync([FromBody]DeviceInformationEventsFilter filter)
        {

            try
            {
                //Result
                var returnResult = new FirewallPolicyEventResult();

                //Filters
                var brandId = filter.BrandId;

                var inLicenceKey = filter.LicenceKey;

                var inSeatKey = filter.SeatKey;

                //Check Authorization status 
                if (!IsAuthorised(inLicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                //Environment selection
                var isDev = IsDevEnvironment(inLicenceKey);
                string apikey = isDev ? _configuration["CEMSDEV:ApiKey"] : _configuration["CEMS:ApiKey"];
                string apiurl = isDev ? _configuration["CEMSDEV:Url"] : _configuration["CEMS:Url"];

                ProductType productType = (ProductType)(filter.OS);

                //Device Type Determination
                var _event = "";

                var _subevent = "";

                if (productType == ProductType.SC_SS_Win)
                {
                    _event = "04";
                    _subevent = "01";
                }

                //Prepare Payload
                string txtResult = "";

                var stringPayload = "";

                stringPayload = JsonConvert.SerializeObject(new GetEventsSpecFilter
                {
                    BrandId = brandId,
                    LicenceKey = inLicenceKey,
                    SeatKey = inSeatKey,
                    Events = _event,
                    SubEvents = _subevent
                });


                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiurl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            txtResult = await response.Content.ReadAsStringAsync();
                        }

                        if (!string.IsNullOrEmpty(txtResult))
                        {
                            var result = JsonConvert.DeserializeObject<GetEventsResult>(txtResult);

                            if (result.s == "true")
                            {
                                if (result.d.Length == 0)
                                {
                                    returnResult.result = 0;
                                    return Json(returnResult); // no records found
                                }
                                else
                                {
                                    var latestMatchedEvent = result.d.OrderByDescending(x=>x.EventDate).ToArray()[0];

                                    if (productType == ProductType.SC_SS_Win)
                                    {
                                        var firewallPolicyEvent = JsonConvert.DeserializeObject<FirewallPolicyEvent[]>(latestMatchedEvent.Description);
                                        returnResult.result = 1;
                                        returnResult.firewallPolicyEvent = firewallPolicyEvent[0];
                                        return Json(returnResult);
                                    }


                                }

                            }
                            else
                            {
                                returnResult.result = -1;
                                return Json(returnResult); ; //API Failed
                            }

                        }

                        returnResult.result = -2;
                        return Json(returnResult); ; //something went wrong

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("devapi_FirewallPolicyEventAsync"));
                        returnResult.result = -3;
                        return Json(returnResult); ; //unhandled exception


                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("devapi_FirewallPolicyEventAsync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
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

        private bool IsDevEnvironment(string licenceKey)
        {


            var subscription = _subscriptionService.GetByLicenceKey(licenceKey);

            if (subscription == null) return false;

            if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
                return false;

            return true;
        }

    }

    public class DeviceInformationEventsFilter
    {
        public string BrandId { get; set; }
        public string LicenceKey { get; set; }
        public string SeatKey { get; set; }
        public int OS { get; set; } // corresponds to ProductType enum

    }
    public class GetEventsFilter
    {
        public string BrandId { get; set; }
        public string LicenceKey { get; set; }
        public string SeatKey { get; set; }
    }
    public class GetEventsSpecFilter
    {
        public string BrandId { get; set; }
        public string LicenceKey { get; set; }
        public string SeatKey { get; set; }
        public string Events { get; set; }
        public string SubEvents { get; set; }
    }
    public enum ProductType
    {
        SC_SS_Win = 1,
        SC_SS_MacOs = 2,
        SC_SS_Android = 3,
        SC_SS_iOS = 4

    }

    public static class StringExtensions
    {
        public static int ConvertToInt(this string input)
        {
            int result;

            return int.TryParse(input, out result) ? result : 0;
        }
        public static DateTime ConvertToDate(this string value, string format)
        {
            DateTime result;

            return DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result : DateTime.MinValue;
        }
    }
}
