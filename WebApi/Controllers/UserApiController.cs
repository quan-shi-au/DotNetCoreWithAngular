using Microsoft.AspNetCore.Mvc;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Partner;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ent.manager.WebApi.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using ent.manager.Services.Subscription;
using System.Net;
using ent.manager.Services.User;
using Microsoft.AspNetCore.Identity;
using ent.manager.WebApi.Results;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ent.manager.Entity.Model;

namespace ent.manager.WebApi.Controllers
{
    public class UserApiController : Controller
    {

        private IEnterpriseClientService _enterpriseClientService;
        private IPartnerService _partnerService;
        private ISubscriptionService _subscriptionService;
        private IUserService _userService;
        private ILogger<UserApiController> _logger { get; set; }

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IConfigurationRoot _configuration;

        public UserApiController(IEnterpriseClientService enterpriseClientService,
            IPartnerService partnerService,
            ISubscriptionService subscriptionService,
            IUserService userService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
             ILogger<UserApiController> logger)
        {
            _enterpriseClientService = enterpriseClientService;
            _partnerService = partnerService;
            _subscriptionService = subscriptionService;
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = CommonHelper.GetConfigurationObject();
        }


        /// <summary>
        /// NO TOKEN DETAIL EXTRACTION
        /// this method doesnt care about the role of the caller (token) as long as its authorized
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var managerUser = _userService.GetById(id);

                if (managerUser == null)
                {
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserDoesntExistGet,
                        d = ""
                    });
                }

                var user = await _userManager.FindByEmailAsync(managerUser.Username);

                if (user == null)
                {
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserDoesntExistGet,
                        d = ""
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);


                var role = roles.FirstOrDefault();


                if (managerUser.PartnerId == null)
                {
                    return Json(new
                    {
                        Id = managerUser.Id,
                        Username = managerUser.Username,
                        FirstName = managerUser.Firstname,
                        LastName = managerUser.Lastname,
                        Role = role
                    });
                }
                else
                {

                    var partner = _partnerService.GetById(managerUser.PartnerId.Value);

                    if (managerUser.EnterpriseId == null)
                    {


                        return Json(new
                        {
                            c = ResultCode.Success,
                            d = new
                            {
                                Id = managerUser.Id,
                                Username = managerUser.Username,
                                FirstName = managerUser.Firstname,
                                LastName = managerUser.Lastname,
                                Role = role,
                                Partner = new { id = partner.Id, name = partner.Name },

                            }
                        });
                    }
                    else
                    {

                        var enterprise = _enterpriseClientService.GetById(managerUser.EnterpriseId.Value);



                        return Json(new
                        {
                            c = ResultCode.Success,
                            d = new
                            {
                                Id = managerUser.Id,
                                Username = managerUser.Username,
                                FirstName = managerUser.Firstname,
                                LastName = managerUser.Lastname,
                                Role = role,
                                Partner = new { id = partner.Id, name = partner.Name },
                                Enterprise = new { id = enterprise.Id, name = enterprise.Name }
                            }
                        });
                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("userapi_getbyid"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }



        }


        /// <summary>
        /// NO TOKEN DETAIL EXTRACTION
        /// this method doesnt care about the role of the caller (token) as long as its authorized
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> GetFullName([FromBody] dynamic value)
        {
            try
            {
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json

                var username = UserJsonEntity["username"].Value;

                User managerUser = _userService.GetByUsername(username);

                var fullname = "";


                if (managerUser == null)
                {

                    var user = await _userManager.FindByEmailAsync(username);

                    if (user == null)
                    {
                        return Json(new
                        {
                            c = ResultCode.UserResultCodes.UserDoesntExistGet,
                            d = ""
                        });
                    }
                    else
                    {
                        fullname = username;
                    }

                }
                else
                {
                    fullname = managerUser.Firstname + " " + managerUser.Lastname;
                }

                return Json(new
                {
                    c = ResultCode.Success,
                    d = new
                    {
                        Fullname = fullname

                    }
                });

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("userapi_getfullname"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        /// <summary>
        /// TOKEN DETAIL EXTRACTION
        /// this method filters based on the role of the caller (token)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
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


                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                //EOF Extract API Called Info

                var identityUsers = _userManager.Users;

                if (TokenDetails.Role.ToLower() == "ec")
                {

                    var users = _userService.GetAll().Where(x => x.EnterpriseId == managerTokenUser.EnterpriseId);
                    var partners = _partnerService.GetAll();
                    var enterprises = _enterpriseClientService.GetAll();


                    //left outer join user and partners
                    var query = from user in users

                                join iusrs in identityUsers
                                on user.UserId equals iusrs.Id

                                join partner in partners
                                on user.PartnerId equals partner.Id
                                into pjoin

                                join ec in enterprises
                                on user.EnterpriseId equals ec.Id
                                into ecjoin

                                from partner in pjoin.DefaultIfEmpty()

                                from ec in ecjoin.DefaultIfEmpty()

                                select new
                                {
                                    Id = user.Id,
                                    Username = user.Username,
                                    FirstName = user.Firstname,
                                    LastName = user.Lastname,
                                    Role = user.Role,
                                    Partner = partner == null ? new { id = "", name = "" } : new { id = partner.Id.ToString(), name = partner.Name },
                                    Enterprise = ec == null ? new { id = "", name = "" } : new { id = ec.Id.ToString(), name = ec.Name },
                                    Status = iusrs.LockoutEnd != null ? "locked" : iusrs.EmailConfirmed ? "active" : "pending"
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query
                    });

                }
                else if (TokenDetails.Role.ToLower() == "partner")
                {

                    var users = _userService.GetAll().Where(x => x.PartnerId == managerTokenUser.PartnerId);
                    var partners = _partnerService.GetAll();
                    var enterprises = _enterpriseClientService.GetAll();

                    //left outer join user and partners
                    var query = from user in users

                                join iusrs in identityUsers
                                on user.UserId equals iusrs.Id

                                join partner in partners
                                on user.PartnerId equals partner.Id
                                into pjoin

                                join ec in enterprises
                                on user.EnterpriseId equals ec.Id
                                into ecjoin

                                from partner in pjoin.DefaultIfEmpty()

                                from ec in ecjoin.DefaultIfEmpty()

                                select new
                                {
                                    Id = user.Id,
                                    Username = user.Username,
                                    FirstName = user.Firstname,
                                    LastName = user.Lastname,
                                    Role = user.Role,
                                    Partner = partner == null ? new { id = "", name = "" } : new { id = partner.Id.ToString(), name = partner.Name },
                                    Enterprise = ec == null ? new { id = "", name = "" } : new { id = ec.Id.ToString(), name = ec.Name },
                                    Status = iusrs.LockoutEnd != null ? "locked" : iusrs.EmailConfirmed ? "active" : "pending"
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query
                    });
                }

                else if (TokenDetails.Role.ToLower() == "admin")
                {
                    var users = _userService.GetAll();
                    var partners = _partnerService.GetAll();
                    var enterprises = _enterpriseClientService.GetAll();

                    //left outer join user and partners
                    var query = from user in users

                                join iusrs in identityUsers
                                on user.UserId equals iusrs.Id

                                join partner in partners
                                on user.PartnerId equals partner.Id
                                into pjoin

                                join ec in enterprises
                                on user.EnterpriseId equals ec.Id
                                into ecjoin

                                from partner in pjoin.DefaultIfEmpty()

                                from ec in ecjoin.DefaultIfEmpty()

                                select new
                                {
                                    Id = user.Id,
                                    Username = user.Username,
                                    FirstName = user.Firstname,
                                    LastName = user.Lastname,
                                    Role = user.Role,
                                    Partner = partner == null ? new { id = "", name = "" } : new { id = partner.Id.ToString(), name = partner.Name },
                                    Enterprise = ec == null ? new { id = "", name = "" } : new { id = ec.Id.ToString(), name = ec.Name }
                                    ,
                                    Status = iusrs.LockoutEnd != null ? "disabled" : iusrs.EmailConfirmed ? "active" : "pending"
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
                _logger.LogError(ex.GetLogText("userapi_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }

        class UsersListFilters
        {
            public string fn { get; set; }
            public string ln { get; set; }
            public string un { get; set; }
            public string eid { get; set; }
            public string pid { get; set; }
            public string role { get; set; }

        }

        [HttpPost]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public IActionResult GetAllPaged([FromBody]dynamic value)
        {
            //Extract API Called Info
            try
            {
                IEnumerable<User> users = new List<User>();

                var TokenDetails = User.Claims.GetTokenDetails();

                var pageSize = int.Parse(_configuration["PageSize"]);


                string svalue = Convert.ToString(value);

                dynamic listQuery = JsonConvert.DeserializeObject(svalue);

                int pageIndex = Convert.ToInt32(listQuery["i"].Value);

                var filtersJson = listQuery["f"];

                UsersListFilters filters = JsonConvert.DeserializeObject<UsersListFilters>(filtersJson.ToString());

                if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(string.Empty);
                }


                var managerTokenUser = _userService.GetByUsername(TokenDetails.Username);

                //EOF Extract API Called Info

                var identityUsers = _userManager.Users;

                if (TokenDetails.Role.ToLower() == "ec")
                {
                    var usersAll = _userService.GetAllPaged(filters.fn, filters.ln, filters.un, filters.eid, filters.pid, filters.role).Where(x => x.EnterpriseId == managerTokenUser.EnterpriseId);
                    var total = usersAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (pageIndex == 1)
                    {
                        users = usersAll.Take(pageSize);
                    }
                    else
                    {
                        users = usersAll.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                    }

                    var partners = _partnerService.GetAll();
                    var enterprises = _enterpriseClientService.GetAll();


                    //left outer join user and partners
                    var query = from user in users

                                join iusrs in identityUsers
                                on user.UserId equals iusrs.Id

                                join partner in partners
                                on user.PartnerId equals partner.Id
                                into pjoin

                                join ec in enterprises
                                on user.EnterpriseId equals ec.Id
                                into ecjoin

                                from partner in pjoin.DefaultIfEmpty()

                                from ec in ecjoin.DefaultIfEmpty()

                                select new
                                {
                                    Id = user.Id,
                                    Username = user.Username,
                                    FirstName = user.Firstname,
                                    LastName = user.Lastname,
                                    Role = user.Role,
                                    Partner = partner == null ? new { id = "", name = "" } : new { id = partner.Id.ToString(), name = partner.Name },
                                    Enterprise = ec == null ? new { id = "", name = "" } : new { id = ec.Id.ToString(), name = ec.Name },
                                    Status = iusrs.LockoutEnd != null ? "locked" : iusrs.EmailConfirmed ? "active" : "pending"
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query,
                        i = pageIndex,
                        t = total,
                        p = pagecount
                    });

                }
                else if (TokenDetails.Role.ToLower() == "partner")
                {
                    var usersAll = _userService.GetAllPaged(filters.fn, filters.ln, filters.un, filters.eid, filters.pid, filters.role).Where(x => x.PartnerId == managerTokenUser.PartnerId);
                    var total = usersAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (pageIndex == 1)
                    {
                        users = usersAll.Take(pageSize);
                    }
                    else
                    {
                        users = usersAll.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                    }
                    var partners = _partnerService.GetAll();
                    var enterprises = _enterpriseClientService.GetAll();

                    //left outer join user and partners
                    var query = from user in users

                                join iusrs in identityUsers
                                on user.UserId equals iusrs.Id

                                join partner in partners
                                on user.PartnerId equals partner.Id
                                into pjoin

                                join ec in enterprises
                                on user.EnterpriseId equals ec.Id
                                into ecjoin

                                from partner in pjoin.DefaultIfEmpty()

                                from ec in ecjoin.DefaultIfEmpty()

                                select new
                                {
                                    Id = user.Id,
                                    Username = user.Username,
                                    FirstName = user.Firstname,
                                    LastName = user.Lastname,
                                    Role = user.Role,
                                    Partner = partner == null ? new { id = "", name = "" } : new { id = partner.Id.ToString(), name = partner.Name },
                                    Enterprise = ec == null ? new { id = "", name = "" } : new { id = ec.Id.ToString(), name = ec.Name },
                                    Status = iusrs.LockoutEnd != null ? "locked" : iusrs.EmailConfirmed ? "active" : "pending"
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query,
                        i = pageIndex,
                        t = total,
                        p = pagecount
                    });
                }

                else if (TokenDetails.Role.ToLower() == "admin")
                {
                    var usersAll = _userService.GetAllPaged(filters.fn, filters.ln, filters.un, filters.eid, filters.pid, filters.role);
                    var total = usersAll.Count();
                    var pagecount = (total / pageSize) + 1;

                    if (pageIndex == 1)
                    {
                        users = usersAll.Take(pageSize);
                    }
                    else
                    {
                        users = usersAll.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                    }


                    var partners = _partnerService.GetAll();
                    var enterprises = _enterpriseClientService.GetAll();

                    //left outer join user and partners
                    var query = from user in users

                                join iusrs in identityUsers
                                on user.UserId equals iusrs.Id

                                join partner in partners
                                on user.PartnerId equals partner.Id
                                into pjoin

                                join ec in enterprises
                                on user.EnterpriseId equals ec.Id
                                into ecjoin

                                from partner in pjoin.DefaultIfEmpty()

                                from ec in ecjoin.DefaultIfEmpty()

                                select new
                                {
                                    Id = user.Id,
                                    Username = user.Username,
                                    FirstName = user.Firstname,
                                    LastName = user.Lastname,
                                    Role = user.Role,
                                    Partner = partner == null ? new { id = "", name = "" } : new { id = partner.Id.ToString(), name = partner.Name },
                                    Enterprise = ec == null ? new { id = "", name = "" } : new { id = ec.Id.ToString(), name = ec.Name }
                                    ,
                                    Status = iusrs.LockoutEnd != null ? "disabled" : iusrs.EmailConfirmed ? "active" : "pending"
                                };

                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = query,
                        i = pageIndex,
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
                _logger.LogError(ex.GetLogText("userapi_getall"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> Add([FromBody]dynamic value)
        {

            try
            {


                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json

                var username = UserJsonEntity["username"].Value;

                var firstname = UserJsonEntity["firstname"].Value;

                var lastname = UserJsonEntity["lastname"].Value;

                string role = UserJsonEntity["role"].Value;

                string domain = UserJsonEntity["domain"].Value;

                int? partnerid = null;

                int? enterpriseid = null;

                try
                {
                    partnerid = Convert.ToInt32(UserJsonEntity["partnerid"].Value);
                }
                catch
                { }

                try
                {
                    enterpriseid = Convert.ToInt32(UserJsonEntity["enterpriseid"].Value);
                }
                catch
                { }

                if (role != "partner" && role != "ec")
                {
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserAddInvalidRole,
                        d = ""
                    });
                }



                // Identity

                var newUser = new IdentityUser
                {
                    UserName = username,
                    Email = username
                };

                var userCreationResult = await _userManager.CreateAsync(newUser);

                if (userCreationResult.Succeeded)
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);


                    //  verify token, if verified
                    // show reset password page
                    // if not show invalid email token message
                    var tokenVerificationUrl = domain + _configuration["PresentationConfirmEmailURL"] + "?id=" + newUser.Id + "&token=" + emailConfirmationToken;

                    foreach (var error in userCreationResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    var _partnerenterprisetext = "N/A";
                    var _partnerenterprisename = "N/A";

                    try
                    {
                        if (role.ToLower() == "partner") //&& enterpriseid == null && partnerid != null)
                        {
                            var partner = _partnerService.GetById(partnerid.Value);
                            _partnerenterprisename = partner.Name;

                            _partnerenterprisetext = "Partner";
                        }
                        else if (role.ToLower() == "ec")// && enterpriseid != null && partnerid != null)
                        {

                            var ent = _enterpriseClientService.GetById(enterpriseid.Value);
                            _partnerenterprisename = ent.Name;

                            _partnerenterprisetext = "Enterprise";
                        }
                    }
                    catch
                    {
                    }

                    await _userManager.AddToRoleAsync(newUser, role);

                    // manager User
                    _userService.Add(new Entity.Model.User()
                    {
                        Username = username,
                        Firstname = firstname,
                        Lastname = lastname,
                        PartnerId = partnerid,
                        EnterpriseId = enterpriseid,
                        Domain = domain,
                        UserId = newUser.Id,
                        Role = role.ToLower()
                    });

                    try
                    {
                        EmailHelper.SendWelcomeEmail(_configuration["entSenderEmail"], username, "ent Enterprise Management Platform – User Registration", tokenVerificationUrl, fname: firstname, lname: lastname, partnerenterprisetext: _partnerenterprisetext, partnerenterprisename: _partnerenterprisename);
                        _logger.LogWarning($"Email verification token sent to username: {username} tokenVerificationUrl: {tokenVerificationUrl}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetLogText("userapi_add_sendwelcomeemail"));

                    }


                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = ""
                    });


                }
                else
                {
                    foreach (var err in userCreationResult.Errors)
                    {
                        if (err.Code == "DuplicateEmail" || err.Code == "DuplicateName" || err.Code == "DuplicateUserName")
                        {
                            return Json(new
                            {
                                c = ResultCode.UserResultCodes.UsernameAlreadyUsed,
                                d = ""
                            });
                        }


                    }

                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserFailedToAdd,
                        d = ""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("userapi_add"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }


        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> Delete([FromBody]dynamic value)
        {
            try
            {
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json

                var username = UserJsonEntity["username"].Value;



                var managerUser = _userService.GetByUsername(username);



                if (managerUser == null)
                {
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserDoesntExistForDelete,
                        d = ""
                    });
                }


                // delete manager User

                _userService.Delete(managerUser);

                var user = await _userManager.FindByEmailAsync(username);

                // delete identity user

                var userDeleteResult = await _userManager.DeleteAsync(user);

                if (userDeleteResult.Succeeded)
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
                        c = ResultCode.UserResultCodes.UserFailedToDelete,
                        d = ""
                    });
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("userapi_delete"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> Lock([FromBody]dynamic value)
        {

            try
            {
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json

                var username = UserJsonEntity["username"].Value;

                var user = await _userManager.FindByEmailAsync(username);


                var userLockoutResult = await _userManager.SetLockoutEnabledAsync(user, true);

                if (userLockoutResult.Succeeded)
                {
                    var userLockoutEndDateResult = await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.UtcNow.AddYears(1)));

                    if (userLockoutEndDateResult.Succeeded)
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
                            c = ResultCode.UserResultCodes.UserFailedToDisable,
                            d = ""
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserFailedToDisable,
                        d = ""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("userapi_lock"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }


        }

        [HttpPost]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> UnLock([FromBody]dynamic value)
        {
            try
            {
                string svalue = Convert.ToString(value);

                dynamic UserJsonEntity = JsonConvert.DeserializeObject(svalue);

                //Extract Json

                var username = UserJsonEntity["username"].Value;

                var user = await _userManager.FindByEmailAsync(username);



                var userLockoutEndDateResult = await _userManager.SetLockoutEndDateAsync(user, null);

                if (userLockoutEndDateResult.Succeeded)
                {
                    var userLockoutResult = await _userManager.SetLockoutEnabledAsync(user, false);
                    if (userLockoutResult.Succeeded)
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
                            c = ResultCode.UserResultCodes.UserFailedToEnable,
                            d = ""
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserFailedToEnable,
                        d = ""
                    });
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("userapi_unlock"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }


        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmailToken(string id, string token)
        {
            try
            {
                _logger.LogWarning($"received email token user id: {id} token: {token}");

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("VerifyEmailTokenFailed|user, with id:" + id + "- token: " + token + "::null");

                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserVerifyECTUserDoesntExist,
                        d = ""
                    });
                }

                var isLocked = await _userManager.IsLockedOutAsync(user);
                if (isLocked)
                {
                    _logger.LogWarning("VerifyEmailTokenFailed|user, with id:" + id + "- token: " + token + "::isLocked");
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserVerifyECTUserIsLocked,
                        d = ""
                    });
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogWarning("VerifyEmailTokenFailed|user, with id:" + id + "- token: " + token + "::EmailConfirmed");
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserVerifyECTLinkOpened,
                        d = ""
                    });
                }

                var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, token);

                if (!emailConfirmationResult.Succeeded)
                {
                    var content = emailConfirmationResult.Errors.Select(error => error.Description).Aggregate((allErrors, error) => allErrors += ", " + error);
                    _logger.LogWarning("VerifyEmailTokenFailed|user, with id:" + id + "- token: " + token + "::emailConfirmationResult.Errors: " + content);

                    return Content(content);
                }

                _logger.LogWarning($"Email token verified successfully. user id: {id} token: {token}");

                //this should inform the presentation to pass through to the set password page
                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                _logger.LogWarning($"password reset token sent to browser {passwordResetToken}");

                return Json(new
                {
                    c = ResultCode.Success,
                    d = Content(passwordResetToken)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("VerifyEmailTokenFailed|user, with id:" + id + "- token: " + token + "::Exception");
                _logger.LogError(ex.GetLogText("userapi_verifyemailtoken"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }

        [HttpPost]
        public async Task<IActionResult> SendResetPassword([FromBody]dynamic value)
        {
            try
            {

                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var email = partnerJsonEntity["username"].Value;

                IdentityUser user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserSendResetUserDoesntExist,
                        d = ""
                    });

                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                //var passwordResetUrl = Url.Action("ConfirmResetPassword", "UserApi", new { id = user.Id, token = passwordResetToken }, Request.Scheme);

                var manageruser = _userService.GetByUsername(user.Email);

                var passwordResetUrl = manageruser.Domain + _configuration["ResetEmailURL"] + "?id=" + user.Id + "&token=" + passwordResetToken;

                var _fname = "";
                var _lname = "";

                if (manageruser != null)
                {
                    _fname = manageruser.Firstname;
                    _lname = manageruser.Lastname;

                }

                _logger.LogWarning($"Password reset email sent. passwordResetUrl: {passwordResetUrl}");

                EmailHelper.SendPasswordSetEmail(_configuration["entSenderEmail"], email, "ent Enterprise Management Platform – Reset your Password", passwordResetUrl, fname: _fname, lname: _lname);

                return Json(new
                {
                    c = ResultCode.Success,
                    d = new { id = user.Id, token = passwordResetToken }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("userapi_sendpasswordreset"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }

        }

        [HttpPost]
        public async Task<IActionResult> SendWelcomeEmail([FromBody]dynamic value)
        {
            try
            {

                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var email = partnerJsonEntity["username"].Value;

                IdentityUser identityUser = await _userManager.FindByEmailAsync(email);

                if (identityUser == null)
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserSendWelcomeUserDoesntExist,
                        d = ""
                    });


                User managerUser = _userService.GetByUsername(email);

                if (managerUser == null)
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserSendWelcomeUserDoesntExist,
                        d = ""
                    });

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);


                //  verify token, if verified
                // show reset password page
                // if not show invalid email token message
                var tokenVerificationUrl = managerUser.Domain + _configuration["PresentationConfirmEmailURL"] + "?id=" + identityUser.Id + "&token=" + emailConfirmationToken;

                 

                var _fname = "";
                var _lname = "";

                if (managerUser != null)
                {
                    _fname = managerUser.Firstname;
                    _lname = managerUser.Lastname;

                }

                _logger.LogInformation($"Confirmation email sent. Confirmation URL: {tokenVerificationUrl}");



                var _partnerenterprisetext = "N/A";
                var _partnerenterprisename = "N/A";
             

                try
                {
                    if (managerUser.Role.ToLower() == "partner") //&& enterpriseid == null && partnerid != null)
                    {
                       
                        var partner = _partnerService.GetById(managerUser.PartnerId.Value);
                       _partnerenterprisename = partner.Name;

                         _partnerenterprisetext = "Partner";
                    }
                    else if (managerUser.Role.ToLower() == "ec")// && enterpriseid != null && partnerid != null)
                    {

                        var ent = _enterpriseClientService.GetById(managerUser.EnterpriseId.Value);
                        _partnerenterprisename = ent.Name;

                        _partnerenterprisetext = "Enterprise";
                    }
                }
                catch
                {
                }


                EmailHelper.SendWelcomeEmail(_configuration["entSenderEmail"], email, 
                    "ent Enterprise Management Platform – User Registration - Resent",
                    tokenVerificationUrl, 
                    fname: _fname, 
                    lname: _lname,
                    partnerenterprisetext: _partnerenterprisetext,
                    partnerenterprisename: _partnerenterprisename);

                return Json(new
                {
                    c = ResultCode.Success,
                    d = new { id = identityUser.Id, token = emailConfirmationToken }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("userapi_sendwelcomeemail"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });

            }

        }

        [HttpPost]
        public async Task<IActionResult> ConfirmResetPassword([FromBody]dynamic value)
        {

            try
            {

                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                string id = partnerJsonEntity["id"].Value;
                string token = partnerJsonEntity["token"].Value;
                var password = partnerJsonEntity["password"].Value;
                var repassword = partnerJsonEntity["repassword"].Value;

                _logger.LogWarning($"received user reset passsword request user id: {id} token: {token}");

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    throw new InvalidOperationException();

                if (password != repassword)
                {

                    _logger.LogWarning("ConfirmResetPassword|user, with id:" + id + "- token: " + token + "::passwordmissmatch");
                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserSetPassMissmatch,
                        d = ""
                    });
                }

                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, password);
                if (!resetPasswordResult.Succeeded)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var error in resetPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        sb.AppendLine(error.Description);
                    }

                    _logger.LogWarning("ConfirmResetPassword|user, with id:" + id + "- token: " + token + "::errors:" + sb.ToString());

                    return Json(new
                    {
                        c = ResultCode.UserResultCodes.UserSetPassSetFailed,
                        d = sb.ToString()
                    });
                }

                _logger.LogWarning($"User reset passsword request successful! user id: {id} token: {token}");

                return Json(new
                {
                    c = ResultCode.Success,
                    d = ""
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("userapi_confirmresetpassword"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ex.Message
                });
            }

        }


    }
}
