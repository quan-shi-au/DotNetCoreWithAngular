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

namespace ent.manager.WebApi.Controllers
{
    public class PartnerApiController : Controller
    {

        private IPartnerService _partnerService;
        private static IConfigurationRoot _configuration;
        private ILogger<PartnerApiController> _logger { get; set; }

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public PartnerApiController(IPartnerService partnerService, ILogger<PartnerApiController> logger)
        {
            _partnerService = partnerService;
            _logger = logger;
            _configuration = CommonHelper.GetConfigurationObject();

        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetById(int id)
        {
            var partner = _partnerService.GetById(id);
            
            return Json(new
            {
                Id = partner.Id,
                Name = partner.Name,
                Location = partner.Location
            });
        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAll()
        {
           
           
            try
            {
                var partners = _partnerService.GetAll();

                return Json(new
                {
                    c = ResultCode.Success,
                    d = partners
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("partnerapi_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }


           
        }

        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllPaged(int i)
        {

            IEnumerable<Partner> partners = new List<Partner>();
            try
            {
                var pageSize = int.Parse(_configuration["PageSize"]);

                var partnersAll = _partnerService.GetAll();
                var total = partnersAll.Count();
                var pagecount = (total / pageSize) + 1;

                if (i == 1)
                {
                     partners = _partnerService.GetAll().Take(pageSize);

                }
                else
                {
                    partners = _partnerService.GetAll().Skip(pageSize * (i-1)).Take(pageSize);
                }
             

                return Json(new
                {
                    c = ResultCode.Success,
                    d = partners.OrderBy(x=>x.Name),
                    i = i,
                    t = total,
                    p = pagecount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("partnerapi_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }



        }

        [HttpDelete]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult Delete(int id)
        {
            try
            {
                var partner = _partnerService.GetById(id);
                var result = _partnerService.Delete(partner);

                return Json(new
                {
                    result = result
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("partnerapi_delete"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public  IActionResult Add([FromBody]dynamic value)
        {
            var name = "";

            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                 name = partnerJsonEntity["name"].Value;

                var location = partnerJsonEntity["location"].Value;

                var addResult = _partnerService.Add(new ent.manager.Entity.Model.Partner() { Name = name, Location = location });

                if(addResult)
                {
                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = ""
                    });

                }
                else
                {
                    return Json(new
                    {
                        c = ResultCode.PartnerResultCodes.PartnerFailedToAdd,
                        d = ""
                    });
                }

                 
            }
            
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("partnerapi_add"));

                var partner = _partnerService.GetByName(name);
                
                if(partner != null)
                {
                    return Json(new
                    {
                        c = ResultCode.PartnerResultCodes.PartnerNameAlreadyExists,
                        d = ex.Message
                    });

                }
               
               

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        


        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult Update([FromBody]dynamic value)
        {
            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var name = partnerJsonEntity["name"].Value;

                var location = partnerJsonEntity["location"].Value;

                int id = Convert.ToInt32(partnerJsonEntity["id"].Value);

                var partner = _partnerService.GetById(id);

                //todo error, partner doesn't exist [route not used]

                partner.Name = name;

                partner.Location = location;

                var result = _partnerService.Update(partner);


                return Json(new
                {
                    result = result
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("partnerapi_update"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
          
        }

        
    }
}
