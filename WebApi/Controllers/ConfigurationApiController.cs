using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using ent.manager.Services.Product;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using ent.manager.WebApi.Helpers;
using Microsoft.Extensions.Configuration;

namespace ent.manager.WebApi.Controllers
{
    public class ConfigurationApiController : Controller
    {

     
        private ILogger<LookupApiController> _logger { get; set; }
        private static IConfigurationRoot _configuration;

        public ConfigurationApiController(ILogger<LookupApiController> logger)
        {
            _logger = logger;
            _configuration = CommonHelper.GetConfigurationObject();
        }

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetPageSize( )
        {
            try
            {
                return Json(new
                {
                    c = ResultCode.Success,
                    d = int.Parse(_configuration["PageSize"])
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("configurationapi_getpagesize"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }
      
        }

  
 
 

        
    }
}
