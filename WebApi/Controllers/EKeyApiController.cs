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
using ent.manager.Services.Subscription;
using ent.manager.Services.Encryption;

namespace ent.manager.WebApi.Controllers
{
    public class EKeyApiController : Controller
    {

        private ISubscriptionService _subscriptionService;
        private IEKeyService _eKeyService;
        private static IConfigurationRoot _configuration;
        private ILogger<EKeyApiController> _logger { get; set; }

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public EKeyApiController(ISubscriptionService subscriptionService, IEKeyService eKeyService, ILogger<EKeyApiController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
            _configuration = CommonHelper.GetConfigurationObject();
            _eKeyService = eKeyService;

        }

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetKey(int subId)
        {
            var subscription = _subscriptionService.GetById(subId);
            var ekey = _eKeyService.GetActive(subscription.EnterpriseClientId);
            try
            {
                return Json(new
                {
                    c = ResultCode.Success,
                    key = ekey.Key ,
                    vector = ekey.IV
                });

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("ekeyapi_GetKey"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        
          
        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult TestEncrypt([FromBody] dynamic value)
        {


            try
            {
                //var keyByteArray = Convert.FromBase64String(key);
                //var vectorByteArray = Convert.FromBase64String(vector);
                //var valueByteArray = Convert.FromBase64String(encryptedString);
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json


                string subId = UserJsonEntity["subscriptionId"].Value;
                string plainText = UserJsonEntity["plainText"].Value;

                var subscription = _subscriptionService.GetById(int.Parse(subId));

                var ekey = _eKeyService.GetActive(subscription.EnterpriseClientId);

                byte[] encryptedByteArray = _eKeyService.Encrypt(plainText, ekey.Key, ekey.IV);

                return Json(new
                {
                    c = ResultCode.Success,
                    key = ekey.Key,
                    vector = ekey.IV,
                    d = Convert.ToBase64String(encryptedByteArray)
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }


        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult TestDecrypt([FromBody] dynamic value)
        {


            try
            {
                //var keyByteArray = Convert.FromBase64String(key);
                //var vectorByteArray = Convert.FromBase64String(vector);
                //var valueByteArray = Convert.FromBase64String(encryptedString);
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json
    

                string key = UserJsonEntity["key"].Value;
                string vector = UserJsonEntity["vector"].Value;
                string encryptedString = UserJsonEntity["encryptedString"].Value; // this should be a base64 encoded

                string result =  _eKeyService.Decrypt(encryptedString, key, vector);

                return Json(new
                {
                    c = "",
                    d = result
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }

       


    }
}
