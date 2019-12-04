using Microsoft.AspNetCore.Mvc;
using System;
using ent.manager.Services.Partner;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.Services.Subscription;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ent.manager.WebApi.Controllers
{
    public class HomeController : Controller
    {

        /// <summary>
        /// Our Links Repostory implementation.
        /// </summary>
        private IPartnerService _partnerService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private ISubscriptionService _subscriptionService;
        private readonly IFileProvider _fileProvider;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        //private static Logger _logger = LogManager.GetCurrentClassLogger();
        private   ILogger<HomeController> _logger { get; set; }

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="linksRepository">IoC resolution for our Repository class.</param>
        public HomeController(IPartnerService partnerService,
            ILicenceEnvironmentService licenceEnvironmentService,
            ISubscriptionService subscriptionService,
            ILogger<HomeController> logger, 
            IFileProvider fileProvider, 
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _partnerService = partnerService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _subscriptionService = subscriptionService;
            _logger = logger;
            _fileProvider = fileProvider;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        /// <summary>
        /// Our Homepage/Landing page.
        /// </summary>
        /// <returns></returns>
        // GET: /
        [HttpGet]
        public IActionResult Index()
        {
            try
            {


                var les = _licenceEnvironmentService.GetAll();

                ViewBag.DBStatus = "Ok";

                ViewBag.RequestScheme = HttpContext.Request.Scheme;
            }
            catch  
            {

                ViewBag.DBStatus = "Down";
            }


            return View();
        }

        [HttpGet]
        public   IActionResult Logs ()
        {
                return View();
        }

        [HttpPost]
        public  IActionResult Logs([FromBody]dynamic value)
        {
            try
            {
                string svalue = Convert.ToString(value);

                dynamic partnerJsonEntity = JsonConvert.DeserializeObject(svalue);

                var username = partnerJsonEntity["un"].Value;
                var password = partnerJsonEntity["pw"].Value;

                //Auth
                var authResult = IsValidUserAndPasswordCombination(username, password);


                if (authResult.Result.result)
                {
                    var contents = _fileProvider.GetDirectoryContents("Logs");
                    return Json(new
                    {
                        contents
                    });
                }
                else
                {
                    return Json("failed");
                }
            


            }
            catch  
            {
                return Json("failed");

            }

        }

       

        private struct IsValidComboResult
        {
            public bool result { get; set; }
            public string role { get; set; }
            public bool isLocked { get; set; }
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
                return result;
            }

           

            result.result = true;
            var rolesList = await _userManager.GetRolesAsync(user);
            result.role = rolesList[0]; //theres always only one role for the user 

            if(result.role != "admin")
            {
                result.result = false;
            }

            return result;
        }

        private string GenerateToken(string username, string role)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890ABCDEF")), //the secret that needs to be at least 16 characeters long for HmacSha256
                                             SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public IActionResult Download(string n)
        {

            var contents = _fileProvider.GetDirectoryContents("Logs");

            foreach (var item in contents)
            {
                if (item.Name == n)
                {
                    var stream1 = item.CreateReadStream();

                    var response = File(stream1, "application/octet-stream", n); // FileStreamResult
                    return response;
                }
            }

            return View(); ;


        }


    }
}
