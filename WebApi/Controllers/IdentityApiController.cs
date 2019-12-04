using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ent.manager.WebApi.Helpers;

namespace ent.manager.WebApi.Controllers
{
    public class IdentityApiController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ILogger<IdentityApiController> _logger { get; set; }

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public IdentityApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger<IdentityApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        //[Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> CreateUser([FromBody]dynamic value)
        {
             

            string svalue = Convert.ToString(value);

            dynamic credJsonEntity = JsonConvert.DeserializeObject(svalue);

            var un = credJsonEntity["un"].Value;

            var pw = credJsonEntity["pw"].Value;

            var assignToRole = false;

            try
            {
                if (!string.IsNullOrEmpty(credJsonEntity["role"].Value))
                {
                    assignToRole = true;
                }
            }
            catch(Exception ex)
            { _logger.LogError(ex.GetLogText("identity_createuser")); }


            var newUser = new IdentityUser
            {
                UserName = un,
                Email = un
            };

            var userCreationResult = await _userManager.CreateAsync(newUser, pw);

            if (userCreationResult.Succeeded)
            {

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var tokenVerificationUrl = Url.Action("VerifyEmail", "Account", new { id = newUser.Id, token = emailConfirmationToken }, Request.Scheme);

                foreach (var error in userCreationResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                if (assignToRole)
                {
                  await _userManager.AddToRoleAsync(newUser, credJsonEntity["role"].Value);
                }

                return Json(new
                {
                    result = "user created"
                });

            }
            else
            {
                return Json(new
                {
                    result = "user creation failed"
                });
            }

        }


        [HttpPost]
        //[Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> CreateRole([FromBody]dynamic value)
        {
            string svalue = Convert.ToString(value);

            dynamic credJsonEntity = JsonConvert.DeserializeObject(svalue);

            var name = credJsonEntity["name"].Value;

            var newRole = new IdentityRole
            {
                Name = name, 
            };

            var roleCreationResult = await _roleManager.CreateAsync(newRole);

            if(roleCreationResult.Succeeded)
            {

                return Json(new
                {
                    result = "role created"
                });

            }
            else
            {
                return Json(new
                {
                    result = "role creation failed"
                });
            }

        }

        [HttpPost]
        //[Authorize(Roles = "admin", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> AssignUserToRole([FromBody]dynamic value)
        {
            string svalue = Convert.ToString(value);

            dynamic credJsonEntity = JsonConvert.DeserializeObject(svalue);

            var un = credJsonEntity["un"].Value;
            var role = credJsonEntity["role"].Value;


            var user = await _userManager.FindByEmailAsync(un);

            var rolesList = await _userManager.GetRolesAsync(user);

            
            foreach (var roleItem in rolesList)
            {
                await _userManager.RemoveFromRoleAsync(user, roleItem);
            }
    
            await _userManager.AddToRoleAsync(user, role);
            await _userManager.UpdateAsync(user);



            return Json(new
            {
                result = string.Format(@"user {0} assigned to role: {1}", un, role)
                });

        }

    }
}
