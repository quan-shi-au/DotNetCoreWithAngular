using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ent.manager.WebApi.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net;
using ent.manager.Services.User;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using ent.manager.Services.Subscription;
using ent.manager.Services.Encryption;
using ent.manager.Entity.Model;

namespace ent.manager.WebApi.Controllers
{
    public class UserDataApiController : Controller
    {

        private IUserDataService _userDataService;
        private ISubscriptionService _subscriptionService;
        private IEKeyService _eKeyService;
        private ILogger<UserApiController> _logger { get; set; }

        public UserDataApiController(IUserDataService userDataService, ISubscriptionService subscriptionService,
            ILogger<UserApiController> logger, IEKeyService eKeyService)
        {
            _userDataService = userDataService;
            _logger = logger;
            _subscriptionService = subscriptionService;
            _eKeyService = eKeyService;
        }

        [HttpPost]
        public IActionResult Add([FromBody]dynamic value)
        {

            try
            {
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                string firstname = UserJsonEntity["fn"];
                string lastname = UserJsonEntity["ln"];
                string licencekey = UserJsonEntity["lk"];
                var seatkey = UserJsonEntity["sk"];
                string optionaldata = UserJsonEntity["opt"];
                var devicetype = UserJsonEntity["dt"];
                var devicemodel = UserJsonEntity["dm"];


                //get the enterprise encryption key
                Subscription subscription = _subscriptionService.GetByLicenceKey(licencekey);
                if (subscription == null)
                {
                    return Json(new
                    {
                        c = ResultCode.UserDataResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });
                }

                EKey eKey = _eKeyService.GetActive(subscription.EnterpriseClientId);

                if (eKey != null)
                {
                    _userDataService.Add(new Entity.Model.UserData()
                    {
                        FirstName = GetEncryptedString(firstname, eKey),
                        LastName = GetEncryptedString(lastname, eKey),
                        LicenceKey = licencekey,
                        SeatKey = seatkey,
                        OptionalData = string.IsNullOrEmpty(optionaldata)? null : GetEncryptedString(optionaldata, eKey),
                        DeviceType = devicetype,
                        DeviceModel = devicemodel
                    });

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = true
                    });

                }
                else
                {
                    return Json(new
                    {
                        c = ResultCode.UserDataResultCodes.EkeyDoesntExist,
                        d = ""
                    });
                }




            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("userdataapi_adduserdata"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAll()
        {
            //Extract API Called Info
            try
            {
                var TokenDetails = User.Claims.GetTokenDetails();


                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }


                var userDataList = _userDataService.GetAll();

                //left outer join user and partners
                var query = from userData in userDataList

                            select new
                            {
                                Id = userData.Id,
                                FirstName = userData.FirstName,
                                LastName = userData.LastName,
                                LicenceKey = userData.LicenceKey,
                                SeatKey = userData.SeatKey,
                                Optional = userData.OptionalData,
                                DeviceType = userData.DeviceType,
                                DeviceModel = userData.DeviceModel,
                                CreationDate = userData.CreationTime


                            };

                return Json(new
                {
                    c = ResultCode.Success,
                    d = query
                });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("userdataapi_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }

        private byte[] GetEncryptedString(string plainText, EKey ekey)
        {
          
            return _eKeyService.Encrypt(plainText, ekey.Key, ekey.IV);
        }



    }
}
