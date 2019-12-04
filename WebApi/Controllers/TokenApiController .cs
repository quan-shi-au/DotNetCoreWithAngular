using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using ent.manager.WebApi.Helpers;
using System.Net;
using Microsoft.Extensions.Logging;
using ent.manager.WebApi.Results;
using Microsoft.Extensions.Configuration;

namespace ent.manager.WebApi.Controllers
{


    public class TokenApiController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private ILogger<TokenApiController> _logger { get; set; }
        private IConfigurationRoot _configuration;

        private struct IsValidComboResult
        {
            public bool result { get; set; }
            public string role { get; set; }
            public bool isLocked { get; set; }
        }

        public TokenApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<TokenApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = CommonHelper.GetConfigurationObject();
        }

        [HttpPost]
        public IActionResult Get([FromBody]dynamic value)
        {
            try
            { //Extract
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var username = partnerJsonEntity["un"].Value;
                var password = partnerJsonEntity["pw"].Value;

                //Auth
                var authResult = IsValidUserAndPasswordCombination(username, password);

                // Return Locked
                if (authResult.Result.isLocked)
                    return Json(new
                    {
                        c = ResultCode.TokenResultCodes.UserLockedOut,
                        d = ""
                    });

                // Return Success
                if (authResult.Result.result && !authResult.Result.isLocked)
                {
                    return Json(new
                    {
                        c = ResultCode.Success,
                        d = new ObjectResult(GenerateToken(username, authResult.Result.role)).Value
                    });
                }
                 

                // Return UnAuth
                return Json(new
                {
                    c = ResultCode.TokenResultCodes.UserUnAuthenticated,
                    d = ""
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("tokenapi_getbyid"));
                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ""
                });
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> RegenerateAsync([FromBody]dynamic value)
        {

            try
            {

            

            //Extract
            string svalue = Convert.ToString(value);

            dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

            var TokenDetails = User.Claims.GetTokenDetails();


            if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
            {
                    return Json(new
                    {
                        c = ResultCode.TokenResultCodes.UserUnAuthenticated,
                        d = ""
                    });
                }

            var user = await _userManager.FindByEmailAsync(TokenDetails.Username);
            if (user == null)
            {

                    return Json(new
                    {
                        c = ResultCode.TokenResultCodes.UserUnAuthenticated,
                        d = ""
                    });
                }

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                    return Json(new
                    {
                        c = ResultCode.TokenResultCodes.UserLockedOut,
                        d = ""
                    });
                }


            // Return Success
                return Json(new
                {
                    c = ResultCode.Success,
                    d = new ObjectResult(GenerateToken(TokenDetails.Username, TokenDetails.Role)).Value
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.GetLogText("tokenapi_regenerateasync"));

                return Json(new
                {
                    c = ResultCode.GenericException,
                    d = ""
                });
            }

        }

        private async Task<IsValidComboResult> IsValidUserAndPasswordCombination(string un, string pw)
        {
            var result = new IsValidComboResult() { result = false, role = "", isLocked = false };
            var user = await _userManager.FindByEmailAsync(un);
            if (user == null)
            {
                 
                return result;
            }
     
            var passwordSignInResult = await _signInManager.PasswordSignInAsync(user, pw, isPersistent: false, lockoutOnFailure: false);
            if (!passwordSignInResult.Succeeded)
            {
                if(passwordSignInResult.IsLockedOut) result.isLocked = true;

                return result;
            }

            //IsLocked
            try
            {
                var isLocked = await _userManager.IsLockedOutAsync(user);

                if (isLocked)
                {
                    result.result = false;
                    result.isLocked = true;
                    return result;
                }
            }
            catch
            {


            }

            result.result = true;
            var rolesList = await _userManager.GetRolesAsync(user);
            result.role = rolesList[0]; //theres always only one role for the user 

            return result;
        }

        private string GenerateToken(string username, string role)
        {
          

            var tokenTimeOutMinutes = 60;
            int.TryParse(_configuration["TokenTimeOutMinutes"], out tokenTimeOutMinutes);

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(tokenTimeOutMinutes)).ToUnixTimeSeconds().ToString()),
            };

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890ABCDEF")), //the secret that needs to be at least 16 characeters long for HmacSha256
                                             SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //[HttpPost]
        //[Authorize(Roles = "admin,partner,ec", AuthenticationSchemes = "Jwt")]
        //public async Task<IActionResult> End()
        //{


        //    try
        //    {
        //        //Extract
        //        var TokenDetails = User.Claims.GetTokenDetails();


        //        if (string.IsNullOrEmpty(TokenDetails.Role) || string.IsNullOrEmpty(TokenDetails.Username))
        //        {
        //            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //            return Json(string.Empty);
        //        }


                

        //        await _signInManager.SignOutAsync();

 
        //    }
        //    catch (Exception)
        //    {
        //        return Content("Sign out failed");

        //    }

        //    return Content("User signed out");
          
        //}
    }
}
