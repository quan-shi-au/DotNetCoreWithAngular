using Microsoft.AspNetCore.Mvc;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner; 
using ent.manager.WebApi.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net;
using ent.manager.Services.User;
using ent.manager.WebApi.Results;
using ent.manager.Services.Subscription;
using Microsoft.Extensions.Logging;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.Services.Product;

namespace ent.manager.WebApi.Controllers
{
    public class DashboardApiController : Controller
    {

        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        private IUserService _userService;
        private ISubscriptionService _subscriptionService;
        private ILogger<DashboardApiController> _logger { get; set; }

        private ILicenceEnvironmentService _licenceEnvironmentService;

        private IProductService _productService;


        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public DashboardApiController(IEnterpriseClientService enterpriseClientService,
            IPartnerService partnerService, 
            IUserService userService,
            ISubscriptionService subscriptionService,
             ILogger<DashboardApiController> logger,
              ILicenceEnvironmentService licenceEnvironmentService,
              IProductService productService)
        {
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
            _userService = userService;
            _subscriptionService = subscriptionService;
            _logger = logger;
            _licenceEnvironmentService = licenceEnvironmentService;
            _productService = productService;
        }

      

        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
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
                                    ProductName = _productService.GetByWId(subscription.Product).Name,
                                    LicencingEnvironment = subscription.LicencingEnvironment,
                                    LicencingEnvironmentName = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment).Name,
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
                                };


                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = new { subscriptions = query, partners = partner, ec = enterprise }
                    });
                }

                 else   if (TokenDetails.Role.ToLower() == "partner")
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
                                    ProductName = _productService.GetByWId(subscription.Product).Name,
                                    LicencingEnvironment = subscription.LicencingEnvironment,
                                    LicencingEnvironmentName = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment).Name,
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
                                    CancelationTime = subscription.CancelationTime
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = new { subscriptions = query, partners = partner, ec = enterpriseClientsList }
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
                                    ProductName = _productService.GetByWId(sub.Product).Name,
                                    LicencingEnvironment = sub.LicencingEnvironment,
                                    LicencingEnvironmentName = _licenceEnvironmentService.GetBymanagerId(sub.LicencingEnvironment).Name,
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
                                    CancelationTime = sub.CancelationTime
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = new { subscriptions = query, partners = partners, ec = enterpriseClients }
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

                _logger.LogError(ex.GetLogText("dashboard_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }
           
        }
         


    }
}
