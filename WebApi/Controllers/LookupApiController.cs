using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using ent.manager.Services.Product;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using ent.manager.WebApi.Helpers;

namespace ent.manager.WebApi.Controllers
{
    public class LookupApiController : Controller
    {

        private IProductService _productService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private ILogger<LookupApiController> _logger { get; set; }

        public LookupApiController(IProductService productService, ILicenceEnvironmentService licenceEnvironmentService, ILogger<LookupApiController> logger)
        {
            _productService = productService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _logger = logger;

        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllProducts( )
        {
            try
            {
                var products = _productService.GetAll();

                return Json(new
                {
                    c = ResultCode.Success,
                    d = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("lookupapi_getproducts"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }
      
        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllLicenceEnvironments()
        {
            try
            {
                var licenceEnvironments = _licenceEnvironmentService.GetAll();


                return Json(new
                {
                    c = ResultCode.Success,
                    d = licenceEnvironments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("lookupapi_getalllicenceenvironment"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
       
        }
 
 

        
    }
}
