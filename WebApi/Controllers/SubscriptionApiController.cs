using Microsoft.AspNetCore.Mvc;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner;
using Newtonsoft.Json;
using ent.manager.WebApi.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using ent.manager.Services.Subscription;
using System.Net;
using ent.manager.Entity.Model;
using ent.manager.Services.User;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.Licence.Helpers;
using ent.manager.Licence.Model;
using ent.manager.Services.Product;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using static ent.manager.Entity.Model.wEnum;
using System.Collections.Generic;
using ent.manager.Services.SubscriptionAuth;
using MailKit.Net.Smtp;

namespace ent.manager.WebApi.Controllers
{
    public class SubscriptionApiController : Controller
    {

        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        private ISubscriptionService _subscriptionService;
        private ISubscriptionAuthService _subscriptionAuthService;
        private IUserService _userService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private IProductService _productService;
        private ILogger<SubscriptionApiController> _logger { get; set; }
        private readonly UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;
        private IConfigurationRoot _configuration;

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public SubscriptionApiController(IEnterpriseClientService enterpriseClientService,
            IPartnerService partnerService,
            ISubscriptionService subscriptionService,
            IUserService userService, ILicenceEnvironmentService licenceEnvironmentService,
             IProductService productService, ILogger<SubscriptionApiController> logger,
             UserManager<IdentityUser> userManager,
             ISubscriptionAuthService subscriptionAuthService)
        {
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
            _subscriptionService = subscriptionService;
            _userService = userService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _productService = productService;
            _logger = logger;
            _userManager = userManager;
            _subscriptionAuthService = subscriptionAuthService;
            _configuration = ent.manager.WebApi.Helpers.CommonHelper.GetConfigurationObject();
        }

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetById(int id)
        {
            try
            {
                var subscription = _subscriptionService.GetById(id);

                if (subscription == null)
                    return Json(new
                    {
                        c = ResultCode.SubscriptionResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });



                var enterprise = _enterpriseClientService.GetById(subscription.EnterpriseClientId);

                var partner = _partnerService.GetById(enterprise.PartnerId);

                var product = _productService.GetByWId(subscription.Product);


                return Json(new
                {
                    c = ResultCode.Success,
                    d = new
                    {
                        Id = subscription.Id,
                        Name = subscription.Name,
                        EnterpriseClientId = subscription.EnterpriseClientId,
                        Product = subscription.Product,
                        ProductName = product.Name,
                        LicencingEnvironment = subscription.LicencingEnvironment,
                        BrandId = subscription.BrandId,
                        Campaign = subscription.Campaign,
                        SeatCount = subscription.SeatCount,
                        CoreAuthUsername = subscription.CoreAuthUsername,
                        RegAuthUsername = subscription.RegAuthUsername,
                        Status = subscription.Status,
                        LicenceKey = subscription.LicenceKey,
                        ClientDownloadLocation = subscription.ClientDownloadLocation,
                        Partner = new { id = partner.Id, name = partner.Name },
                        Enterprise = new { id = enterprise.Id, name = enterprise.Name },
                        CreationTime = subscription.CreationTime,
                        CancelationTime = subscription.CancelationTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("subscriptionapi_getbyid"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAll()
        {
            try
            {
                // Extract API Called Info
                var TokenDetails = User.Claims.GetTokenDetails();


                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }


                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                //EOF Extract API Called Info


                if (TokenDetails.Role.ToLower() == "ec")
                {
                  
                    var enterprise = _enterpriseClientService.GetById(managerTokenUser.EnterpriseId.Value);
                    var subscriptionsList = _subscriptionService.GetByEnterpriseId(enterprise.Id);
                    var partner = _partnerService.GetById(enterprise.PartnerId);
                    var query = from subscription in subscriptionsList
                                select new
                                {
                                    Id = subscription.Id,
                                    Name = subscription.Name,
                                    EnterpriseClientId = subscription.EnterpriseClientId,
                                    Product = subscription.Product,
                                    LicencingEnvironment = subscription.LicencingEnvironment,
                                    BrandId = subscription.BrandId,
                                    Campaign = subscription.Campaign,
                                    SeatCount = subscription.SeatCount,
                                    CoreAuthUsername = subscription.CoreAuthUsername,
                                    RegAuthUsername = subscription.RegAuthUsername,
                                    Status = subscription.Status,
                                    LicenceKey = subscription.LicenceKey,
                                    ClientDownloadLocation = subscription.ClientDownloadLocation,
                                    Partner = new { id = partner.Id, name = partner.Name },
                                    Enterprise = new { id = enterprise.Id, name = enterprise.Name },
                                    CreationTime = subscription.CreationTime,
                                    CancelationTime = subscription.CancelationTime,
                                    SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                                    SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
                                };


                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query
                    });

                }
                else if (TokenDetails.Role.ToLower() == "partner")
                {
                
                    var partner = _partnerService.GetById(managerTokenUser.PartnerId.Value);
                    var enterpriseClientsList = _enterpriseClientService.GetByPartnerId(partner.Id);
                    var subscriptions = _subscriptionService.GetAll();

                    var subscriptionsList = from ec in enterpriseClientsList
                                            join sub in subscriptions
                                             on ec.Id equals sub.EnterpriseClientId
                                            select new
                                            {
                                                sub.Id,
                                                sub.Name,
                                                sub.EnterpriseClientId,
                                                sub.Product,
                                                sub.LicencingEnvironment,
                                                sub.BrandId,
                                                sub.Campaign,
                                                sub.SeatCount,
                                                sub.CoreAuthUsername,
                                                sub.RegAuthUsername,
                                                sub.Status,
                                                sub.LicenceKey,
                                                sub.ClientDownloadLocation,
                                                enterpriseid = ec.Id,
                                                enterprisename = ec.Name,
                                                sub.CreationTime,
                                                sub.CancelationTime
                                            };



                    var query = from subscription in subscriptionsList
                                select new
                                {
                                    Id = subscription.Id,
                                    Name = subscription.Name,
                                    EnterpriseClientId = subscription.EnterpriseClientId,
                                    Product = subscription.Product,
                                    LicencingEnvironment = subscription.LicencingEnvironment,
                                    BrandId = subscription.BrandId,
                                    Campaign = subscription.Campaign,
                                    SeatCount = subscription.SeatCount,
                                    CoreAuthUsername = subscription.CoreAuthUsername,
                                    RegAuthUsername = subscription.RegAuthUsername,
                                    Status = subscription.Status,
                                    LicenceKey = subscription.LicenceKey,
                                    ClientDownloadLocation = subscription.ClientDownloadLocation,
                                    Partner = new { id = partner.Id, name = partner.Name },
                                    Enterprise = new { id = subscription.enterpriseid, name = subscription.enterprisename },
                                    CreationTime = subscription.CreationTime,
                                    CancelationTime = subscription.CancelationTime,
                                    SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                                    SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query
                    });
                }

                else if (TokenDetails.Role.ToLower() == "admin")
                {
                    var subscriptions = _subscriptionService.GetAll();
                    var partners = _partnerService.GetAll();
                    var enterpriseClients = _enterpriseClientService.GetAll();

                    var resultList = from sub in subscriptions
                                     join ec in enterpriseClients
                                      on sub.EnterpriseClientId equals ec.Id
                                     join partner in partners on ec.PartnerId equals partner.Id
                                     select new
                                     {
                                         sub.Id,
                                         sub.Name,
                                         sub.EnterpriseClientId,
                                         sub.Product,
                                         sub.LicencingEnvironment,
                                         sub.BrandId,
                                         sub.Campaign,
                                         sub.SeatCount,
                                         sub.CoreAuthUsername,
                                         sub.RegAuthUsername,
                                         sub.Status,
                                         sub.LicenceKey,
                                         sub.ClientDownloadLocation,
                                         enterpriseid = ec.Id,
                                         enterprisename = ec.Name,
                                         partnerid = partner.Id,
                                         partnername = partner.Name,
                                         sub.CreationTime,
                                         sub.CancelationTime
                                     };



                    var query = from sub in resultList
                                select new
                                {
                                    Id = sub.Id,
                                    Name = sub.Name,
                                    EnterpriseClientId = sub.EnterpriseClientId,
                                    Product = sub.Product,
                                    LicencingEnvironment = sub.LicencingEnvironment,
                                    BrandId = sub.BrandId,
                                    Campaign = sub.Campaign,
                                    SeatCount = sub.SeatCount,
                                    CoreAuthUsername = sub.CoreAuthUsername,
                                    RegAuthUsername = sub.RegAuthUsername,
                                    Status = sub.Status,
                                    LicenceKey = sub.LicenceKey,
                                    ClientDownloadLocation = sub.ClientDownloadLocation,
                                    Partner = new { id = sub.partnerid, name = sub.partnername },
                                    Enterprise = new { id = sub.enterpriseid, name = sub.enterprisename },
                                    CreationTime = sub.CreationTime,
                                    CancelationTime = sub.CancelationTime,
                                    SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(sub.Id).Username,
                                    SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(sub.Id).Pin
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
                _logger.LogError(ex.GetLogText("subscriptionapi_all"));
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }


        }

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllPaged(int i)
        {
            try
            {
                // Extract API Called Info
                var TokenDetails = User.Claims.GetTokenDetails();

                var pageSize = int.Parse(_configuration["PageSize"]);

                IEnumerable<Subscription> subscriptionsList = new List<Subscription>();

                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }


                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                //EOF Extract API Called Info


                if (TokenDetails.Role.ToLower() == "ec")
                {

                    var enterprise = _enterpriseClientService.GetById(managerTokenUser.EnterpriseId.Value);
                    var subscriptionsListAll =  _subscriptionService.GetByEnterpriseId(enterprise.Id);
                    var total = subscriptionsListAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (i == 1)
                    {
                        subscriptionsList = subscriptionsListAll.Take(pageSize);
                    }
                    else
                    {
                        subscriptionsList = subscriptionsListAll.Skip(pageSize * (i-1)).Take(pageSize);
                    }
     
                    var partner = _partnerService.GetById(enterprise.PartnerId);
                    var query = from subscription in subscriptionsList
                                select new
                                {
                                    Id = subscription.Id,
                                    Name = subscription.Name,
                                    EnterpriseClientId = subscription.EnterpriseClientId,
                                    Product = subscription.Product,
                                    LicencingEnvironment = subscription.LicencingEnvironment,
                                    BrandId = subscription.BrandId,
                                    Campaign = subscription.Campaign,
                                    SeatCount = subscription.SeatCount,
                                    CoreAuthUsername = subscription.CoreAuthUsername,
                                    RegAuthUsername = subscription.RegAuthUsername,
                                    Status = subscription.Status,
                                    LicenceKey = subscription.LicenceKey,
                                    ClientDownloadLocation = subscription.ClientDownloadLocation,
                                    Partner = new { id = partner.Id, name = partner.Name },
                                    Enterprise = new { id = enterprise.Id, name = enterprise.Name },
                                    CreationTime = subscription.CreationTime,
                                    CancelationTime = subscription.CancelationTime,
                                    SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                                    SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
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
                else if (TokenDetails.Role.ToLower() == "partner")
                {

                    var partner = _partnerService.GetById(managerTokenUser.PartnerId.Value);
                    var enterpriseClientsList = _enterpriseClientService.GetByPartnerId(partner.Id);
                    var subscriptionsListAll = _subscriptionService.GetAll();
                    var total = subscriptionsListAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (i == 1)
                    {
                        subscriptionsList = subscriptionsListAll.Take(pageSize);
                    }
                    else
                    {
                        subscriptionsList = subscriptionsListAll.Skip(pageSize * (i-1)).Take(pageSize);
                    }
                        

                    var subs = from ec in enterpriseClientsList
                                            join sub in subscriptionsList
                                             on ec.Id equals sub.EnterpriseClientId
                                            select new
                                            {
                                                sub.Id,
                                                sub.Name,
                                                sub.EnterpriseClientId,
                                                sub.Product,
                                                sub.LicencingEnvironment,
                                                sub.BrandId,
                                                sub.Campaign,
                                                sub.SeatCount,
                                                sub.CoreAuthUsername,
                                                sub.RegAuthUsername,
                                                sub.Status,
                                                sub.LicenceKey,
                                                sub.ClientDownloadLocation,
                                                enterpriseid = ec.Id,
                                                enterprisename = ec.Name,
                                                sub.CreationTime,
                                                sub.CancelationTime
                                            };



                    var query = from subscription in subs
                                select new
                                {
                                    Id = subscription.Id,
                                    Name = subscription.Name,
                                    EnterpriseClientId = subscription.EnterpriseClientId,
                                    Product = subscription.Product,
                                    LicencingEnvironment = subscription.LicencingEnvironment,
                                    BrandId = subscription.BrandId,
                                    Campaign = subscription.Campaign,
                                    SeatCount = subscription.SeatCount,
                                    CoreAuthUsername = subscription.CoreAuthUsername,
                                    RegAuthUsername = subscription.RegAuthUsername,
                                    Status = subscription.Status,
                                    LicenceKey = subscription.LicenceKey,
                                    ClientDownloadLocation = subscription.ClientDownloadLocation,
                                    Partner = new { id = partner.Id, name = partner.Name },
                                    Enterprise = new { id = subscription.enterpriseid, name = subscription.enterprisename },
                                    CreationTime = subscription.CreationTime,
                                    CancelationTime = subscription.CancelationTime,
                                    SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Username,
                                    SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(subscription.Id).Pin
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
                    var subscriptionsListAll = _subscriptionService.GetAll();
                    var total = subscriptionsListAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (i == 1)
                    {
                        subscriptionsList = subscriptionsListAll.Take(pageSize);
                    }
                    else
                    {
                        subscriptionsList = subscriptionsListAll.Skip(pageSize * (i-1)).Take(pageSize);
                    }
               
                    

                    var partners = _partnerService.GetAll();
                    var enterpriseClients = _enterpriseClientService.GetAll();

                    var resultList = from sub in subscriptionsList
                                     join ec in enterpriseClients
                                      on sub.EnterpriseClientId equals ec.Id
                                     join partner in partners on ec.PartnerId equals partner.Id
                                     select new
                                     {
                                         sub.Id,
                                         sub.Name,
                                         sub.EnterpriseClientId,
                                         sub.Product,
                                         sub.LicencingEnvironment,
                                         sub.BrandId,
                                         sub.Campaign,
                                         sub.SeatCount,
                                         sub.CoreAuthUsername,
                                         sub.RegAuthUsername,
                                         sub.Status,
                                         sub.LicenceKey,
                                         sub.ClientDownloadLocation,
                                         enterpriseid = ec.Id,
                                         enterprisename = ec.Name,
                                         partnerid = partner.Id,
                                         partnername = partner.Name,
                                         sub.CreationTime,
                                         sub.CancelationTime
                                     };



                    var query = from sub in resultList
                                select new
                                {
                                    Id = sub.Id,
                                    Name = sub.Name,
                                    EnterpriseClientId = sub.EnterpriseClientId,
                                    Product = sub.Product,
                                    LicencingEnvironment = sub.LicencingEnvironment,
                                    BrandId = sub.BrandId,
                                    Campaign = sub.Campaign,
                                    SeatCount = sub.SeatCount,
                                    CoreAuthUsername = sub.CoreAuthUsername,
                                    RegAuthUsername = sub.RegAuthUsername,
                                    Status = sub.Status,
                                    LicenceKey = sub.LicenceKey,
                                    ClientDownloadLocation = sub.ClientDownloadLocation,
                                    Partner = new { id = sub.partnerid, name = sub.partnername },
                                    Enterprise = new { id = sub.enterpriseid, name = sub.enterprisename },
                                    CreationTime = sub.CreationTime,
                                    CancelationTime = sub.CancelationTime,
                                    SubAuthUn = _subscriptionAuthService.GetBySubscriptionId(sub.Id).Username,
                                    SubAuthPw = _subscriptionAuthService.GetBySubscriptionId(sub.Id).Pin
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
                _logger.LogError(ex.GetLogText("subscriptionapi_all"));
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
                var sub = _subscriptionService.GetById(id);


                if (sub == null)
                    return Json(new
                    {
                        result = "Subscription doesn't exist"
                    });


                var result = _subscriptionService.Delete(sub);

                return Json(new
                {
                    result = result
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("subscriptionapi_delete"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async System.Threading.Tasks.Task<IActionResult> AddAsync([FromBody]dynamic value)
        {
            var name = "";

            try
            {
                string svalue = Convert.ToString(value);

                dynamic EnterpriseClientJsonEntity = JsonConvert.DeserializeObject(svalue);

                name = EnterpriseClientJsonEntity["name"].Value;



                var sub = _subscriptionService.GetByName(name);

                if (sub != null)
                {
                    return Json(new
                    {
                        c = ResultCode.SubscriptionResultCodes.SubscriptionAlreadyExists,
                        d = ""
                    });

                }

                var enterpriseClientId = Convert.ToInt32(EnterpriseClientJsonEntity["enterpriseclientid"].Value);

                var productwid = Convert.ToInt32(EnterpriseClientJsonEntity["product"].Value);

                var licencingEnvironment = Convert.ToInt32(EnterpriseClientJsonEntity["licencingenvironment"].Value);

                var brandId = EnterpriseClientJsonEntity["brandid"].Value;

                var campaign = EnterpriseClientJsonEntity["campaign"].Value;

                var seatCount = Convert.ToInt32(EnterpriseClientJsonEntity["seatcount"].Value);

                var coreauthUsername = EnterpriseClientJsonEntity["coreauthusername"].Value;

                var coreauthPassword = EnterpriseClientJsonEntity["coreauthpassword"].Value;

                var regauthUsername = EnterpriseClientJsonEntity["regauthusername"].Value;

                var regauthPassword = EnterpriseClientJsonEntity["regauthpassword"].Value;

                var clientDownloadLocation = EnterpriseClientJsonEntity["clientdownloadlocation"].Value;


                //Licence Server

                var licenceEnvironment = _licenceEnvironmentService.GetBymanagerId(licencingEnvironment);

                var product = _productService.GetByWId(productwid);

                EnterpriseClient enterprise = _enterpriseClientService.GetById(enterpriseClientId);

                var email = enterprise.Name.Trim().TrimEnd().TrimStart().RemoveSpecialCharacters() + "@ent.com";

                var licenceResult = "";


                if (licencingEnvironment == (int)LicencingEnvironmentEnum.Development)
                {
                    manager.Licence.Proxy.LsxDevReg lsxDevReg = new manager.Licence.Proxy.LsxDevReg(url: licenceEnvironment.RegURL,
                   username: regauthUsername,
                   password: regauthPassword,
                   brandid: brandId,
                   campaignid: campaign,
                   logger: _logger);

                    //Create Licence
                    var createResult = await lsxDevReg.CreateLicenceAsync(email);

                    createResult = @"<Response xmlns='http://tempuri.org/'>" + createResult + "</Response>";

                    licenceResult = LicenceHelper.ExtractLicenceFromCreateResponse(createResult,_logger);

                }
                else if (licencingEnvironment == (int)LicencingEnvironmentEnum.Production)
                {
                    manager.Licence.Proxy.Ls1Reg Ls1Reg = new manager.Licence.Proxy.Ls1Reg(url: licenceEnvironment.RegURL,
                  username: regauthUsername,
                  password: regauthPassword,
                  brandid: brandId,
                  campaignid: campaign,
                  logger: _logger);

                    //Create Licence
                    var createResult = await Ls1Reg.CreateLicenceAsync(email);

                    createResult = @"<Response xmlns='http://tempuri.org/'>" + createResult + "</Response>";

                    licenceResult = LicenceHelper.ExtractLicenceFromCreateResponse(createResult,_logger);


                }


                if (!string.IsNullOrEmpty(licenceResult))
                {

                    //Update Max Seat Count
                    manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: licenceEnvironment.CoreURL,
                    username: coreauthUsername,
                    password: coreauthPassword,
                    brandid: brandId,
                    campaignid: campaign,
                    logger: _logger);


                    try
                    {
                        var resultSetLicenseProductInfo = lsxDevCore.SetLicenseProductInfo(codes: product.Codes, licenceKey: licenceResult, numberofseats: seatCount);
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex.GetLogText("subscriptionapi_AddAsync_Failed_SetLicenseProductInfo"));
                    }

                    var entity = new ent.manager.Entity.Model.Subscription()
                    {
                        Name = name,
                        EnterpriseClientId = enterpriseClientId,
                        Product = productwid,
                        LicencingEnvironment = licencingEnvironment,
                        BrandId = brandId,
                        Campaign = campaign,
                        SeatCount = seatCount,
                        CoreAuthUsername = coreauthUsername,
                        CoreAuthPassword = coreauthPassword,
                        RegAuthUsername = regauthUsername,
                        RegAuthPassword = regauthPassword,
                        ClientDownloadLocation = clientDownloadLocation,
                        Status = true,
                        LicenceKey = licenceResult
                    };
                    var result = _subscriptionService.Add(entity);

                    if (result)
                    {
                        try
                        {
          
                            _subscriptionAuthService.Add(entity);

                        }
                        catch  
                        {
                            return Json(new
                            {
                                c = ResultCode.SubscriptionResultCodes.AddingSubscriptionAuthenticationDetailsFailed,
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
                            c = ResultCode.SubscriptionResultCodes.SubscriptionFailedToAdd,
                            d = ""
                        });
                    }

                }
                else
                {
                    return Json(new
                    {
                        c = ResultCode.SubscriptionResultCodes.SubsscriptionLicenceGenerationFailedr,
                        d = ""
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("subscriptionapi_AddAsync"));

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
            string svalue = Convert.ToString(value);

            dynamic EnterpriseClientJsonEntity = JsonConvert.DeserializeObject(svalue);

            var name = EnterpriseClientJsonEntity["name"].Value;

            var enterpriseClientId = Convert.ToInt32(EnterpriseClientJsonEntity["enterpriseclientid"].Value);

            var product = EnterpriseClientJsonEntity["product"].Value;

            var licencingEnvironment = EnterpriseClientJsonEntity["licencingenvironment"].Value;

            var brandId = EnterpriseClientJsonEntity["brandid"].Value;

            var campaign = EnterpriseClientJsonEntity["campaign"].Value;

            var seatCount = Convert.ToInt32(EnterpriseClientJsonEntity["seatcount"].Value);

            var coreauthUsername = EnterpriseClientJsonEntity["coreauthusername"].Value;

            var coreauthPassword = EnterpriseClientJsonEntity["coreauthpassword"].Value;

            var regauthUsername = EnterpriseClientJsonEntity["regauthusername"].Value;

            var regauthPassword = EnterpriseClientJsonEntity["regauthpassword"].Value;

            var clientDownloadLocation = EnterpriseClientJsonEntity["clientdownloadlocation"].Value;

            var id = Convert.ToInt32(EnterpriseClientJsonEntity["id"].Value);

            Subscription subscriptionObj = _subscriptionService.GetById(id);

            subscriptionObj.Name = name;

            subscriptionObj.EnterpriseClientId = enterpriseClientId;

            subscriptionObj.Product = product;

            subscriptionObj.LicencingEnvironment = licencingEnvironment;

            subscriptionObj.BrandId = brandId;

            subscriptionObj.Campaign = campaign;

            subscriptionObj.SeatCount = seatCount;

            subscriptionObj.CoreAuthPassword = coreauthUsername;

            subscriptionObj.CoreAuthPassword = coreauthPassword;

            subscriptionObj.RegAuthPassword = regauthUsername;

            subscriptionObj.RegAuthPassword = regauthPassword;

            subscriptionObj.ClientDownloadLocation = clientDownloadLocation;


            var result = _subscriptionService.Update(subscriptionObj);


            return Json(new
            {
                result = result
            });
        }


        private void sendSubscriptionCancellationEmail(int enterpriseId, string licenceKey, int productId)
        {

            var product = _productService.GetByWId(productId);

            var enterpriseAdminList = _userService.GetByEnterpriseId(enterpriseId);

            var users = _userManager.Users;

            var receivers = from ea in enterpriseAdminList
                            join u in users
                             on ea.Username equals u.Email
                            where u.EmailConfirmed == true
                            select new
                            {
                                username = u.Email,
                                fname = ea.Firstname,
                                lname = ea.Lastname
                            };


            foreach (var item in receivers)
            {

                EmailHelper.SendSubscriptionCancellationEmail(_configuration["entSenderEmail"],
                    item.username,
                    "ent Enterprise Management Platform – Your Enterprise Subscription has been Cancelled",
                    licenceKey,
                    product.Name,
                    fname: item.fname,
                    lname: item.lname
                    );

            }


        }


        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult Cancel([FromBody]dynamic value)
        {

            try
            {

                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var subid = Convert.ToInt32(partnerJsonEntity["sid"].Value);

                Subscription subscription = _subscriptionService.GetById(subid);

                LicenceEnvironment le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);

                CancelLicenceResult cancellicenceresult = new CancelLicenceResult() { code = -1, result = false };

                if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
                {
                    manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                    username: subscription.CoreAuthUsername,
                    password: subscription.CoreAuthPassword,
                    brandid: subscription.BrandId,
                    campaignid: subscription.Campaign,
                    logger: _logger);

                    cancellicenceresult = lsxDevCore.CancelLicence(subscription.LicenceKey);

                    if (cancellicenceresult.result)
                    {
                        subscription.Status = false;
                        subscription.CancelationTime = DateTime.UtcNow;
                        _subscriptionService.Update(subscription);

                        sendSubscriptionCancellationEmail(subscription.EnterpriseClientId, subscription.LicenceKey, subscription.Product);

                        return Json(new
                        {
                            c = ResultCode.Success,
                            d = ""
                        });
                    }
                    else
                    {
                        if (cancellicenceresult.code == 1000) // customer already disabled
                        {
                            subscription.Status = false;
                            subscription.CancelationTime = DateTime.UtcNow;
                            _subscriptionService.Update(subscription);


                            return Json(new
                            {
                                c = ResultCode.SubscriptionResultCodes.SubscriptionLicenceAlreadyCancel,
                                d = ""
                            });
                        }

                    }
                }
                else if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
                {
                    manager.Licence.Proxy.Ls1Core Ls1Core = new manager.Licence.Proxy.Ls1Core(url: le.CoreURL,
                   username: subscription.CoreAuthUsername,
                   password: subscription.CoreAuthPassword,
                   brandid: subscription.BrandId,
                   campaignid: subscription.Campaign,
                   logger: _logger);

                    cancellicenceresult = Ls1Core.CancelLicence(subscription.LicenceKey);

                    if (cancellicenceresult.result)
                    {
                        subscription.Status = false;
                        subscription.CancelationTime = DateTime.UtcNow;
                        _subscriptionService.Update(subscription);

                        sendSubscriptionCancellationEmail(subscription.EnterpriseClientId, subscription.LicenceKey, subscription.Product);

                        return Json(new
                        {
                            c = ResultCode.Success,
                            d = ""
                        });
                    }
                    else
                    {
                        if (cancellicenceresult.code == 1000) // customer already disabled
                        {
                            subscription.Status = false;
                            subscription.CancelationTime = DateTime.UtcNow;
                            _subscriptionService.Update(subscription);


                            return Json(new
                            {
                                c = ResultCode.SubscriptionResultCodes.SubscriptionLicenceAlreadyCancel,
                                d = ""
                            });
                        }

                    }
                }

                //if we reach here something went wrong
                return Json(new
                {
                    c = ResultCode.SubscriptionResultCodes.SubscriptionFailedToCancel,
                    d = ""
                });

               

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("subscriptionapi_cancel"));
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult SetSeatCount([FromBody]dynamic value)
        {


            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var subid = Convert.ToInt32(partnerJsonEntity["sid"].Value);

                var seatcount = Convert.ToInt32(partnerJsonEntity["scnt"].Value);

                Subscription subscription = _subscriptionService.GetById(subid);

                LicenceEnvironment le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);

                var product = _productService.GetByWId(subscription.Product);

                if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
                {
                    manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                    username: subscription.CoreAuthUsername,
                    password: subscription.CoreAuthPassword,
                    brandid: subscription.BrandId,
                    campaignid: subscription.Campaign,
                    logger:_logger);

                    var result = lsxDevCore.SetLicenseProductInfo(codes: product.Codes, licenceKey: subscription.LicenceKey, numberofseats: seatcount);


                    //Update manager
                    if (result)
                    {
                        subscription.SeatCount = seatcount;
                        _subscriptionService.Update(subscription);


                        return Json(new
                        {
                            c = ResultCode.Success,
                            d = ""
                        });
                    }

                }
                else if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
                {
                    manager.Licence.Proxy.Ls1Core Ls1Core = new manager.Licence.Proxy.Ls1Core(url: le.CoreURL,
                   username: subscription.CoreAuthUsername,
                   password: subscription.CoreAuthPassword,
                   brandid: subscription.BrandId,
                   campaignid: subscription.Campaign,
                   logger: _logger);

                    var result = Ls1Core.SetLicenseProductInfo(codes: product.Codes, licenceKey: subscription.LicenceKey, numberofseats: seatcount);


                    //Update manager
                    if (result)
                    {
                        subscription.SeatCount = seatcount;
                        _subscriptionService.Update(subscription);


                        return Json(new
                        {
                            c = ResultCode.Success,
                            d = ""
                        });
                    }
                }

                return Json(new
                {
                    c = ResultCode.SubscriptionResultCodes.SubscriptionFailedToSetSeatCount,
                    d = ""
                });



            }

            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("subscriptionapi_setseatcount"));
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public IActionResult SendInstructions([FromBody]dynamic value)
        {
            try
            {
                

                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var subid = Convert.ToInt32(partnerJsonEntity["sid"].Value);


                Subscription subscription = _subscriptionService.GetById(subid);

                var product = _productService.GetByWId(subscription.Product);

                var enterpriseAdminList = _userService.GetByEnterpriseId(subscription.EnterpriseClientId);

                var users = _userManager.Users;


                SubscriptionAuth subscriptionAuth = _subscriptionAuthService.GetBySubscriptionId(subscription.Id);

                var receivers = from ea in enterpriseAdminList
                                join u in users
                                 on ea.Username equals u.Email
                                where u.EmailConfirmed == true
                                select new
                                {
                                    username = u.Email,
                                    fname = ea.Firstname,
                                    lname = ea.Lastname
                                };

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect(_configuration["smtp:client"]);
                    client.Authenticate(_configuration["smtp:username"], _configuration["smtp:password"]);

                    foreach (var item in receivers)
                    {

                        if (_configuration["LegacyInstructionsEmail"].Equals("true"))
                        {

                            EmailHelper.SendInstructionsEmailLegacy(_configuration["entSenderEmail"],
                      item.username,
                      "ent Enterprise Management Platform – Your Enterprise Subscription",
                      licenceKey: subscription.LicenceKey,
                      downloadLocation: subscription.ClientDownloadLocation,
                      productName: product.Name,
                      fname: item.fname,
                      lname: item.lname,
                      substartdate: subscription.CreationTime.ToString("dd/MM/yyyy"),
                      maxseatcount: subscription.SeatCount.ToString(), client: client
                      );


                        }
                        else
                        {

                            EmailHelper.SendInstructionsEmail(_configuration["entSenderEmail"],
                      item.username,
                      "ent Enterprise Management Platform – Your Enterprise Subscription",
                      subscriptionAuth.Username,
                      subscriptionAuth.Pin,
                      subscription.ClientDownloadLocation,
                      product.Name,
                      fname: item.fname,
                      lname: item.lname,
                      substartdate: subscription.CreationTime.ToString("dd/MM/yyyy"),
                      maxseatcount: subscription.SeatCount.ToString(), client: client
                      );

                        }
                    }
                }

                if(receivers.Count() > 0)
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
                        c = ResultCode.SubscriptionResultCodes.SubsscriptionSendInstructionsNoReceiver,
                        d = ""
                    });

                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("subscriptionapi_sendinstructions"));
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        [HttpPost]
        public  IActionResult Auth([FromBody]dynamic value)
        {
            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var username = partnerJsonEntity["un"].Value;

                var pin = partnerJsonEntity["pin"].Value;

                var result =  _subscriptionAuthService.AuthenticateGetLicence(username, pin);

                if(!string.IsNullOrEmpty(result))
                {
                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = result
                    });
                }
                else
                {
                    return Json(new
                    {
                        c = ResultCode.SubscriptionResultCodes.InvalidSubscriptionAuthentication,
                        d = result
                    });
                }
              
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
