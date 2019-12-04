using Microsoft.AspNetCore.Mvc;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner;
using Newtonsoft.Json;
using ent.manager.WebApi.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net;
using ent.manager.Services.User;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ent.manager.Entity.Model;
using System.Collections.Generic;
using ent.manager.Services.Encryption;

namespace ent.manager.WebApi.Controllers
{
    public class EnterpriseClientApiController : Controller
    {

        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        private IUserService _userService;
        private ILogger<EnterpriseClientApiController> _logger { get; set; }
        private static IConfigurationRoot _configuration;
        private IEKeyService _eKeyService;

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public EnterpriseClientApiController(IEnterpriseClientService enterpriseClientService, 
            IPartnerService partnerService, 
            IUserService userService, 
            ILogger<EnterpriseClientApiController> logger,
             IEKeyService eKeyService)
        {
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
            _userService = userService;
            _logger = logger;
            _configuration = CommonHelper.GetConfigurationObject();
            _eKeyService = eKeyService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,partner", AuthenticationSchemes = "Jwt")]
        public IActionResult GetById(int id)
        {
            var enterpriseClient = _enterpriseClientService.GetById(id);

            if(enterpriseClient == null)
                return Json(new
                {
                    result = "Enterprise doesn't exist"
                });

            var partner = _partnerService.GetById(enterpriseClient.PartnerId);

            return Json(new
            {
                Id = enterpriseClient.Id,
                Name = enterpriseClient.Name,
                Location = enterpriseClient.Location,
                Partner = new {id = enterpriseClient.PartnerId, name = partner.Name }
            });
        }

        [HttpGet]
        [Authorize(Roles = "admin,partner", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAll()
        {

            try
            {
                var TokenDetails = User.Claims.GetTokenDetails();


                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                //EOF Extract API Called Info


                if (TokenDetails.Role.ToLower() == "partner")
                {
             
                    var partner = _partnerService.GetById(managerTokenUser.PartnerId.Value);
                    var partnerEnterpriseClients = _enterpriseClientService.GetByPartnerId(partner.Id);

                    var query = from ec in partnerEnterpriseClients
                                select new
                                {
                                    Id = ec.Id,
                                    Name = ec.Name,
                                    Location = ec.Location,
                                    Partner = new { id = ec.PartnerId, name = partner.Name }
                                };


                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query
                    });


                }
                else if (TokenDetails.Role.ToLower() == "admin")
                {
                    var EnterpriseClients = _enterpriseClientService.GetAll();
                    var Partners = _partnerService.GetAll();


                    var query = from ec in EnterpriseClients
                                join partner in Partners
                                     on ec.PartnerId equals partner.Id
                                select new
                                {
                                    Id = ec.Id,
                                    Name = ec.Name,
                                    Location = ec.Location,
                                    Partner = new { id = ec.PartnerId, name = partner.Name }
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query
                    });
                }
                else
                {
                    //if code reaches this point something went wrong
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("ec_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
           
        }


        [HttpGet]
        [Authorize(Roles = "admin,partner", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllPaged(int i)
        {

            try
            {
                IEnumerable<EnterpriseClient> enterpriseclients = new List<EnterpriseClient>();

                var TokenDetails = User.Claims.GetTokenDetails();

                var pageSize = int.Parse(_configuration["PageSize"]);

                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                //EOF Extract API Called Info


                if (TokenDetails.Role.ToLower() == "partner")
                {

                    var partner = _partnerService.GetById(managerTokenUser.PartnerId.Value);
                    var enterpriseclientsAll = _enterpriseClientService.GetByPartnerId(partner.Id);
                    var total = enterpriseclientsAll.Count();
                    var pagecount = total/ pageSize;

                    if (i == 1)
                    {
                        enterpriseclients = enterpriseclientsAll.Take(pageSize);
                    }
                    else
                    {
                        enterpriseclients = enterpriseclientsAll.Skip(pageSize * (i-1)).Take(pageSize);

                    }
                    

                    var query = from ec in enterpriseclients
                                select new
                                {
                                    Id = ec.Id,
                                    Name = ec.Name,
                                    Location = ec.Location,
                                    Partner = new { id = ec.PartnerId, name = partner.Name }
                                };


                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query,
                        i = i,
                        t = total, 
                        p = pagecount
                    });


                }
                else if (TokenDetails.Role.ToLower() == "admin")
                {
                    var enterpriseclientsAll = _enterpriseClientService.GetAll();
                    var total = enterpriseclientsAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (i == 1)
                    {
                        enterpriseclients = enterpriseclientsAll.Take(pageSize);
                    }
                    else
                    {
                        enterpriseclients = enterpriseclientsAll.Skip(pageSize * (i-1)).Take(pageSize);
                    }
                       
                    var Partners = _partnerService.GetAll();


                    var query = from ec in enterpriseclients
                                join partner in Partners
                                     on ec.PartnerId equals partner.Id
                                select new
                                {
                                    Id = ec.Id,
                                    Name = ec.Name,
                                    Location = ec.Location,
                                    Partner = new { id = ec.PartnerId, name = partner.Name }
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query,
                        i = i,
                        t = total,
                        p = pagecount
                    });
                }
                else
                {
                    //if code reaches this point something went wrong
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("ec_getall"));

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
                var enterpriseClient = _enterpriseClientService.GetById(id);


                if (enterpriseClient == null)
                    return Json(new
                    {
                        result = "Enterprise doesn't exist"
                    });


                var result = _enterpriseClientService.Delete(enterpriseClient);

                return Json(new
                {
                    result = result
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("ec_delete"));

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

                dynamic EnterpriseClientJsonEntity = JsonConvert.DeserializeObject(svalue);

                name = EnterpriseClientJsonEntity["name"].Value;

                var location = EnterpriseClientJsonEntity["location"].Value;

                var partnerId = EnterpriseClientJsonEntity["pid"].Value;


                var newObj = new ent.manager.Entity.Model.EnterpriseClient() { Name = name, Location = location, PartnerId = Convert.ToInt32(partnerId) };

                var result = _enterpriseClientService.Add(newObj);
 

                if (result)
                {

                    if (!_eKeyService.Save(newObj.Id))
                    {
                        return Json(new
                        {
                            c = ResultCode.EnterpriseResultCodes.EkeyFailedToAdd,
                            d = ""
                        });
                    }

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
                        c = ResultCode.EnterpriseResultCodes.EnterpriseFailedToAdd,
                        d = ""
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("ec_add"));

                var enterprse = _enterpriseClientService.GetByName(name);

                if (enterprse != null)
                {
                    return Json(new
                    {
                        c = ResultCode.EnterpriseResultCodes.EnterpriseNameAlreadyExists,
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

                dynamic EnterpriseClientJsonEntity = JsonConvert.DeserializeObject(svalue);

                var name = EnterpriseClientJsonEntity["name"].Value;

                var location = EnterpriseClientJsonEntity["location"].Value;

                int id = Convert.ToInt32(EnterpriseClientJsonEntity["id"].Value);

                var EnterpriseClient = _enterpriseClientService.GetById(id);

                EnterpriseClient.Name = name;

                EnterpriseClient.Location = location;

                var result = _enterpriseClientService.Update(EnterpriseClient);


                return Json(new
                {
                    result = result
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("ec_update"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
        
        }


        [HttpGet]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult GetByPartnerId(int id)
        {
            try
            {

                var enterpriseClients = _enterpriseClientService.GetByPartnerId(id);

                return Json(new
                {
                    c = ResultCode.Success,
                    d = enterpriseClients
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("ec_getbypartnerid"));
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
           
        }


    }
}
