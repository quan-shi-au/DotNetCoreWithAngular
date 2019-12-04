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
using ent.manager.Services.User;
using System.Net;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner;

namespace ent.manager.WebApi.Controllers
{
    public class LicenceApiController : Controller
    {

        private ISubscriptionService _subscriptionService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private IProductService _productService;
        private IUserService _userService;
        private ILogger<LicenceApiController> _logger { get; set; }
        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public LicenceApiController(ISubscriptionService subscriptionService, IEnterpriseClientService enterpriseClientService, IPartnerService partnerService,ILicenceEnvironmentService licenceEnvironmentService, IProductService productService,IUserService userService, ILogger<LicenceApiController> logger)
        {
            _subscriptionService = subscriptionService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _productService = productService;
            _logger = logger;
            _userService = userService;
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
        }

  
        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> GetLicenceSummaryAsync([FromBody]dynamic value)
        {
            SafeCentralLicenceDetail result = new SafeCentralLicenceDetail();

            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var subid = Convert.ToInt32(partnerJsonEntity["sid"].Value);

                Subscription subscription = _subscriptionService.GetById(subid);

                LicenceEnvironment le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);

                //Check Authorization status 
                if (!IsAuthorised(subscription.LicenceKey))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }

                if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
                {
                    manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                    username: subscription.CoreAuthUsername,
                    password: subscription.CoreAuthPassword,
                    brandid: subscription.BrandId,
                    campaignid: subscription.Campaign, 
                    logger: _logger);


                    result = await lsxDevCore.GetLicenceDetailsAsync(subscription.LicenceKey);

                    return Json(result);

                }
                else if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
                {
                    manager.Licence.Proxy.Ls1Core Ls1Core = new manager.Licence.Proxy.Ls1Core(url: le.CoreURL,
                   username: subscription.CoreAuthUsername,
                   password: subscription.CoreAuthPassword,
                   brandid: subscription.BrandId,
                   campaignid: subscription.Campaign,
                   logger: _logger);

                    result = await Ls1Core.GetLicenceDetailsAsync(subscription.LicenceKey);

                    return Json(result);
                    

                }
            }
            catch (System.InvalidOperationException)
            {
             
                return Json(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("licapi_getlicencesummaryasync"));
                return Json(ex.Message);
            }


            return Json("");




        }


        [HttpPost]
        [Authorize(Roles = "admin, partner, ec", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> DeactivateSeatAsync([FromBody]dynamic value)
        {
            bool result = false;

            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var seatKey = partnerJsonEntity["sk"].Value;

                var subId = Convert.ToInt32(partnerJsonEntity["sid"].Value);

                Subscription subscription = _subscriptionService.GetById(subId);

                if(subscription ==null)
                {
                    return Json(new
                    {
                        c = ResultCode.LicenceResultCodes.SubscriptionDoesntExist,
                        d = ""
                    });
                }

                LicenceEnvironment le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);

                if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
                {



                    manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                    username: subscription.CoreAuthUsername,
                    password: subscription.CoreAuthPassword,
                    brandid: subscription.BrandId,
                    campaignid: subscription.Campaign,
                    logger: _logger);

                    var licresult = await lsxDevCore.GetLicenceDetailsAsync(subscription.LicenceKey);

                    foreach (var item in licresult.ComputersCurrentlyInstalled)
                    {
                        if(item.Seats.Where(x=>x.SeatKey == seatKey).Count() > 0)
                        {
                            foreach (var seat in item.Seats)
                            {
                                result = lsxDevCore.DeactivateSeat(seat.SeatKey);
                            }
                        }

                    }


                    if(result)
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
                            c = ResultCode.LicenceResultCodes.FailedToDeactivateSeat,
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

                    var licresult = await Ls1Core.GetLicenceDetailsAsync(subscription.LicenceKey);

                    foreach (var item in licresult.ComputersCurrentlyInstalled)
                    {
                        if (item.Seats.Where(x => x.SeatKey == seatKey).Count() > 0)
                        {
                            foreach (var seat in item.Seats)
                            {
                                result = Ls1Core.DeactivateSeat(seat.SeatKey);
                            }
                        }

                    }

             

                    if (result)
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
                            c = ResultCode.LicenceResultCodes.FailedToDeactivateSeat,
                            d = ""
                        });
                    }
                }
            }
            catch (System.InvalidOperationException)
            {

                return Json(new
                {
                    c = ResultCode.Success,
                    d = ""
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("licapi_DeactivateSeat"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

            //something went wrong.

            return Json(new
            {
                c = ResultCode.LicenceResultCodes.FailedToDeactivateSeat,
                d = ""
            });
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
