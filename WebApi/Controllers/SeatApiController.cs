using Microsoft.AspNetCore.Mvc;
using ent.manager.Services.Partner;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Authorization;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using ent.manager.WebApi.Helpers;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections;
using ent.manager.Entity.Model;
using System.Collections.Generic;
using ent.manager.Services.User;
using ent.manager.Entity.Model.Reporting;
using ent.manager.Services.Subscription;
using ent.manager.Services.LicenceEnvironment;
using static ent.manager.Entity.Model.wEnum;
using Microsoft.Extensions.Caching.Memory;
using ent.manager.Services.DeviceTypeDictionary;
using ent.manager.Services.DeviceModelDictionary;
using ent.manager.Services.Encryption;
using ent.manager.Services.EnterpriseClient;
using System.Net;

namespace ent.manager.WebApi.Controllers
{
    public class SeatApiController : Controller
    {

        private IPartnerService _partnerService;
        private IUserDataService _userDataService;
        private ISubscriptionService _subscriptionService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private IDeviceTypeDictionaryService _deviceTypeDictionaryService;
        private IDeviceModelDictionaryService _deviceModelDictionaryService;
        private static IConfigurationRoot _configuration;
        private ILogger<PartnerApiController> _logger { get; set; }
        private IMemoryCache _cache;
        private IEKeyService _eKeyService;
          private IUserService _userService;
       
        private IEnterpriseClientService _enterpriseClientService;
         

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public SeatApiController(IPartnerService partnerService, 
            ILogger<PartnerApiController> logger, 
            IUserDataService userDataService,
            ISubscriptionService subscriptionService,
            ILicenceEnvironmentService licenceEnvironmentService,
            IMemoryCache cache,
            IDeviceTypeDictionaryService deviceTypeDictionaryService,
            IDeviceModelDictionaryService deviceModelDictionaryService,
            IEKeyService eKeyService,
             IEnterpriseClientService enterpriseClientService,
             IUserService userService)
        {
            _partnerService = partnerService;
            _logger = logger;
            _userDataService = userDataService;
            _subscriptionService = subscriptionService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _deviceTypeDictionaryService = deviceTypeDictionaryService;
            _deviceModelDictionaryService = deviceModelDictionaryService;
            _cache = cache;
            _eKeyService = eKeyService;
            _userService = userService;
            _enterpriseClientService = enterpriseClientService;
            _configuration = CommonHelper.GetConfigurationObject();
            FillCache();
        }

        class SeatDetailsFilters
        {
            public string fn { get; set; }
            public string ln { get; set; }
            public string od { get; set; }
            public string dn { get; set; }

        }

       

        private string GetDecryptedString(string base64EncryptedValue, Subscription subscription)
        {
            var result = "";
            try
            {
                var ekey = _eKeyService.GetActive(subscription.EnterpriseClientId);

                var decryptedString = _eKeyService.Decrypt(base64EncryptedValue, ekey.Key, ekey.IV);

                result = decryptedString;

            }
            catch
            {


            }

            return result;
        }

        [HttpPost]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllPaged([FromBody]dynamic value)
        {

            try
            {
                IEnumerable<Partner> partners = new List<Partner>();

                var pageSize = int.Parse(_configuration["PageSize"]);

                string svalue = Convert.ToString(value);

                dynamic listQuery = JsonConvert.DeserializeObject(svalue);

                int subscriptionId = Convert.ToInt32(listQuery["s"].Value);

                int pageIndex = Convert.ToInt32(listQuery["i"].Value);

                var filtersJson = listQuery["f"];

                SeatDetailsFilters filters = JsonConvert.DeserializeObject<SeatDetailsFilters>(filtersJson.ToString());

                var subscription = _subscriptionService.GetById(subscriptionId);

                if(subscription == null)
                {
                    return Json(new
                    {
                        c = ResultCode.SeatResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });
                }

                //Check Authorization status 
                if (!IsAuthorised(subscription.LicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }


                var licObject = GetLicenceObject(subscription);

                var seats =  GetSeats(licObject.Result);

                if (!string.IsNullOrWhiteSpace(filters.fn))
                {
                    //string fname_filter = GetEncryptedBase64(filters.fn.ToLower(), subscription).ToLower();
                    seats =  seats.Where(x => x.FirstName != null && GetDecryptedString(x.FirstName,subscription).ToLower().Contains(filters.fn.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(filters.ln))
                {
                    //string lname_filter = GetEncryptedBase64(filters.ln.ToLower(), subscription).ToLower();
                    seats = seats.Where(x => x.LastName != null && GetDecryptedString(x.LastName, subscription).ToLower().Contains(filters.ln.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(filters.od))
                {
                    //string od_filter = GetEncryptedBase64(filters.od.ToLower(), subscription).ToLower();
                    seats = seats.Where(x => x.OptionalData != null && GetDecryptedString(x.OptionalData,subscription).ToLower().Contains(filters.od.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(filters.dn))
                {
                    seats = seats.Where(x => x.DeviceName != null && x.DeviceName.ToLower().Contains(filters.dn.ToLower()));
                }

                var total = seats.Count();

                var pagecount = (total / pageSize) + 1;

                var pagedSeats = seats.OrderBy(x => x.DeviceName).ThenBy(x=>x.ActivationDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                return Json(new
                {
                    c = ResultCode.Success,
                    d = GetSeatDetailsRecordTranslated(pagedSeats),
                    i = pageIndex,
                    t = total,
                    p = pagecount,

                });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("seatapicontroller_getallpaged"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }



        }



        private async System.Threading.Tasks.Task<Licence.Model.SafeCentralLicenceDetail> GetLicenceObject(Subscription subscription)
        {
            var le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);
 

            if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
            {
                manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                username: subscription.CoreAuthUsername,
                password: subscription.CoreAuthPassword,
                brandid: subscription.BrandId,
                campaignid: subscription.Campaign,
                logger: _logger);

                var result = await lsxDevCore.GetLicenceDetailsAsync(subscription.LicenceKey);
               
                return result;

            }
            else if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
            {
                manager.Licence.Proxy.Ls1Core Ls1Core = new manager.Licence.Proxy.Ls1Core(url: le.CoreURL,
               username: subscription.CoreAuthUsername,
               password: subscription.CoreAuthPassword,
               brandid: subscription.BrandId,
               campaignid: subscription.Campaign,
               logger: _logger);

                var result = await Ls1Core.GetLicenceDetailsAsync(subscription.LicenceKey);
 
                return result;
            }



            //if code reaches here something went wrong
            return null;
        }

        private IQueryable<SeatLine> GetSeats(Licence.Model.SafeCentralLicenceDetail licenceObj)
        {
            var userDataEnumerable = _userDataService.GetAll();

            List<SeatLine> result = new List<SeatLine>();

            if (licenceObj != null)
            {
                foreach (var item in licenceObj.ComputersCurrentlyInstalled)
                {

                    SeatLine seatDetailsReportLine = new SeatLine();

                   

                    if (item.Seats.Count > 0)
                    {

                        var activeseatslist = from lseat in item.Seats.Where(x => x.IsBlocked == false).OrderByDescending(x => x.ActivationDate)
                                         join udseat in userDataEnumerable.OrderByDescending(x => x.CreationTime)
                                         on lseat.SeatKey equals udseat.SeatKey
                                         select lseat;


                        var activeseat = activeseatslist.FirstOrDefault() ;

                        if (activeseat != null)
                        {

                            var userDataRecord = userDataEnumerable.Where(x => x.SeatKey == activeseat.SeatKey).OrderByDescending(x => x.CreationTime).FirstOrDefault();

                            // the following can be gathered from the licence details
                            seatDetailsReportLine.ActivationDate = activeseat.ActivationDate;
                            seatDetailsReportLine.LastUpdateDate = activeseat.LastUpdate;
                    
                        
                            seatDetailsReportLine.OsVersion = item.OperatingSystem;
                            seatDetailsReportLine.DeviceName = item.Name;
                            seatDetailsReportLine.SeatKey = activeseat.SeatKey;
                            seatDetailsReportLine.VersionNumber = activeseat.Version;
                            if (userDataRecord != null) //corresponding user data found
                            {
                               

                                seatDetailsReportLine.DeviceType = userDataRecord.DeviceType;
                                seatDetailsReportLine.FirstName = Convert.ToBase64String(userDataRecord.FirstName);
                                seatDetailsReportLine.LastName = Convert.ToBase64String(userDataRecord.LastName);
                                seatDetailsReportLine.DeviceModel = userDataRecord.DeviceModel;
                                seatDetailsReportLine.OptionalData = userDataRecord.OptionalData == null?string.Empty : Convert.ToBase64String(userDataRecord.OptionalData);


                            }
                            else // corresponding user data not found
                            {

                                seatDetailsReportLine.DeviceType = "";
                                seatDetailsReportLine.FirstName = "";
                                seatDetailsReportLine.LastName = "";
                                seatDetailsReportLine.DeviceModel = "";
                                seatDetailsReportLine.OptionalData = "";

                            }


                            result.Add(seatDetailsReportLine);

                        }

                    }

                }

            }

            return result.ToList().AsQueryable();

        }

        private List<SeatLine> GetSeatDetailsRecordTranslated(List<SeatLine> list)
        {

            try
            {

                list.ForEach(c => {
                    c.DeviceModel = GetValueFromCache(c.DeviceModel);
                    c.DeviceType = GetValueFromCache(c.DeviceType);
                });

                return list;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("seattapi_GetSeatDetailsReportRecordTranslated"));
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

    }
}
