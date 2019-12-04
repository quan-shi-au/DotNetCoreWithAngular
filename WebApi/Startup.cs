using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ent.manager.Data.Repositories;
using ent.manager.Data.Context;
using ent.manager.Services.Partner;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using ent.manager.Services.EnterpriseClient;
using ent.manager.Services.Subscription;
using ent.manager.Services.User;
using ent.manager.Services.Product;
using ent.manager.Services.LicenceEnvironment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ent.manager.Services.Reporting.ReportProcessorRun;
using ent.manager.Services.Reporting.Report;
using ent.manager.Reporting;
using ent.manager.Services.DeviceModelDictionary;
using ent.manager.Services.DeviceTypeDictionary;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using ent.manager.Services.Encryption;
using ent.manager.Services.SubscriptionAuth;
using Microsoft.Extensions.Options;

namespace ent.manager.WebApi
{

    public class ConfirmEmailDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public ConfirmEmailDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<ConfirmEmailDataProtectionTokenProviderOptions> options) : base(dataProtectionProvider, options)
        {
        }
    }

    public class ConfirmEmailDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions { }


    public class Startup
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        private IHostingEnvironment _hostingEnvironment;
        ///   

        #region "Private Methods"

        private const string _certificateBytes = "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";

        /// <summary>
        /// Gets an embedded test certificate to check encryption/decryption functions.
        /// </summary>
        /// <returns>
        /// An <see cref="X509Certificate2"/> that can be used for encryption/decryption.
        /// </returns>
        private X509Certificate2 GetCertificate()
        {
            // Certificate originally generated with
            // makecert -sv privatekey.pvk -n "CN=Key Protection" keyprotection.cer -sky Exchange
            // pvk2pfx /pvk privatekey.pvk /spc keyprotection.cer /pfx keyprotection.pfx
            // The above bytes are the keyprotection.pfx certificate.
            var bytes = Convert.FromBase64String(_certificateBytes);
            return new X509Certificate2(bytes);
        }

        private X509Certificate2 GetClientCertificateFromDisk()
        {
            // string Certificate = @"parental-controls.nonprod-api.in.telstra.com.au.cer";
            string Certificate = @"star_ent_net.pfx";
            // Load the certificate into an X509Certificate object.
            X509Certificate2 cert = new X509Certificate2();

            //cert.Import(Certificate);

            var x509 = new X509Certificate2(File.ReadAllBytes(Certificate), "w0nt0k");
            return cert;
        }

        private void SaveKeys()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection()
                .SetApplicationName("entent")
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Users\DevAdmin\Desktop\Work\Clients\manager\ServerKeys"));
            var services = serviceCollection.BuildServiceProvider();
            var provider = services.GetService<IDataProtectionProvider>();
            var protector = provider.CreateProtector("entent");
            Convert.ToBase64String(protector.Protect(Encoding.UTF8.GetBytes("entent")));
        }

        #endregion

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _hostingEnvironment = env;
        }




        public void ConfigureServices(IServiceCollection services)
        {
            // SaveKeys(); // use to generate keys.xml file
            services.AddMvc();

            services.AddDataProtection()
    .SetApplicationName("entent")
    .PersistKeysToFileSystem(new DirectoryInfo(@"TGK"))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365 * 20))
    .DisableAutomaticKeyGeneration();

            //manager dbcontext
            services.AddDbContext<managerDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("managerConnection")));

            // identity
            services.AddDbContext<IdentityDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("managerConnection"),
               optionsBuilder => optionsBuilder.MigrationsAssembly("ent.manager.WebApi")));


           

            //// identity - configure token generation
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders()
                 .AddTokenProvider<ConfirmEmailDataProtectorTokenProvider<IdentityUser>>("ConfirmEmailDataProtectorTokenProvider");    

            // identity - configure identity options
            services.Configure<IdentityOptions>(o =>
            {
                o.SignIn.RequireConfirmedEmail = true;
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = "ConfirmEmailDataProtectorTokenProvider";
            });

            services.Configure<ConfirmEmailDataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(double.Parse(Configuration["ConfirmationEmailTokenExpiryMinutes"]));
            });

            //FileProvider - Logs
            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;
            services.AddSingleton<IFileProvider>(physicalProvider);

            // IoC bindings
            services.AddScoped<IPartnerService, PartnerService>();
            services.AddScoped<IEnterpriseClientService, EnterpriseClientService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ILicenceEnvironmentService, LicenceEnvironmentService>();
            services.AddScoped<IUserDataService, UserDataService>();
            services.AddScoped<IEKeyService, EKeyService>();
            // Reporting IOC Binding

            services.AddSingleton<ReportProvider>();
            services.AddSingleton<IHostedService, DataRefreshService>();
            services.AddSingleton<IReportProcessor, ReportProcessor>();
            services.AddSingleton<ILogger<ReportProcessor>, Logger<ReportProcessor>>();

            services.AddScoped<IReportProcessorRunService, ReportProcessorRunService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUsageReportService, UsageReportService>();
            services.AddScoped<IDeviceOSReportService, DeviceOSReportService>();
            services.AddScoped<ISeatDetailsReportService, SeatDetailsReportService>();
            services.AddScoped<IDeviceTypeReportService, DeviceTypeReportService>();
            services.AddScoped<IDeviceManufacturerReportService, DeviceManufacturerReportService>();

            services.AddScoped<IDeviceModelDictionaryService, DeviceModelDictionaryService>();
            services.AddScoped<IDeviceTypeDictionaryService, DeviceTypeDictionaryService>();
            services.AddScoped<ISubscriptionAuthService, SubscriptionAuthService>();
            // DBContext IOC Binding

            services.AddScoped<DbContext, managerDbContext>();


            // IoC bindings - Registering Open Generics
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            //CORS
            //services.AddCors();

            //JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Jwt";
                options.DefaultChallengeScheme = "Jwt";
            }).AddJwtBearer("Jwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    //ValidAudience = "the audience you want to validate",
                    ValidateIssuer = false,
                    //ValidIssuer = "the isser you want to validate",

                    ValidateIssuerSigningKey = true,
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the secret that needs to be at least 16 characeters long for HmacSha256")),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890ABCDEF")),

                    ValidateLifetime = true, //validate the expiration and not before values in the token

                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            }); ;

            //Caching
            services.AddMemoryCache();

            #region commented
            // Add our database context into the IoC container.
            //var driver = Configuration["DbDriver"];
            //// Use SQLite?
            //if (driver == "sqlite")
            //{
            //    services.AddDbContext<DatabaseContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            //}
            //// Use MSSQL?
            //if (driver == "mssql")
            //{
            //    services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //}

            #endregion


        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IdentityDbContext dbContext)
        {

            loggerFactory.AddDBLogger(Configuration.GetConnectionString("managerConnection"), Configuration["LogLevel"]);

            // Enable development environment exception page.
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                //// dbContext.Database.Migrate();

                ////CORS // Comment the following when releasing
                //app.UseCors(b => b.WithOrigins("http://localhost:55904/")
                app.UseCors(b => b.WithOrigins("http://52.0.39.249/")
                   .AllowAnyOrigin()
                   .AllowCredentials()
                   .AllowAnyMethod()
                   .AllowAnyHeader());
            }

            // Enable static files (assets to be served etc.)
            app.UseStaticFiles();

           
          

            //Identity
            //app.UseIdentity(); //obsolete replaced with app.UseAuthentication();
            app.UseAuthentication();


            //routes
            app.UseMvc(
                 routes =>
                 {

                     // Pages

                     routes.MapRoute("homepage", "", defaults: new { controller = "Home", action = "Index" });

                     routes.MapRoute("logpage", "logs", defaults: new { controller = "Home", action = "Logs" });

                     routes.MapRoute("downloadpage", "download", defaults: new { controller = "Home", action = "Download" });

                     // Test

                     //  routes.MapRoute("api-getbyid", "api/v1/getbyid", defaults: new { controller = "Api", action = "GetById" });

                     // Partners

                     // routes.MapRoute("api-partners-getbyid", "api/v1/partners/get/{id}", defaults: new { controller = "PartnerApi", action = "GetById" });

                     routes.MapRoute("api-partners-getall", "api/v1/partners/fetch", defaults: new { controller = "PartnerApi", action = "GetAll" });

                     routes.MapRoute("api-partners-getallp", "api/v1/partners/fetchp/{i}", defaults: new { controller = "PartnerApi", action = "GetAllPaged" });

                     // routes.MapRoute("api-partners-delete", "api/v1/partners/delete/{id}", defaults: new { controller = "PartnerApi", action = "Delete" });

                     routes.MapRoute("api-partners-add", "api/v1/partners/add", defaults: new { controller = "PartnerApi", action = "Add" });

                     // routes.MapRoute("api-partners-update", "api/v1/partners/update", defaults: new { controller = "PartnerApi", action = "Update" });

                     // Enterprise Clients

                     // routes.MapRoute("api-ec-getbyid", "api/v1/ec/get/{id}", defaults: new { controller = "EnterpriseClientApi", action = "GetById" });

                     routes.MapRoute("api-ec-getall", "api/v1/ec/fetch", defaults: new { controller = "EnterpriseClientApi", action = "GetAll" });

                     routes.MapRoute("api-ec-getallp", "api/v1/ec/fetchp/{i}", defaults: new { controller = "EnterpriseClientApi", action = "GetAllPaged" });

                     // routes.MapRoute("api-ec-delete", "api/v1/ec/delete/{id}", defaults: new { controller = "EnterpriseClientApi", action = "Delete" });

                     routes.MapRoute("api-ec-add", "api/v1/ec/add", defaults: new { controller = "EnterpriseClientApi", action = "Add" });

                     //     routes.MapRoute("api-ec-update", "api/v1/ec/update", defaults: new { controller = "EnterpriseClientApi", action = "Update" });

                     routes.MapRoute("api-ec-getbypartnerid", "api/v1/ec/getbypid/{id}", defaults: new { controller = "EnterpriseClientApi", action = "GetByPartnerId" });

                     // Subscriptions

                     routes.MapRoute("api-sub-getbyid", "api/v1/sub/get/{id}", defaults: new { controller = "SubscriptionApi", action = "GetById" });

                     routes.MapRoute("api-sub-getall", "api/v1/sub/fetch", defaults: new { controller = "SubscriptionApi", action = "GetAll" });

                     routes.MapRoute("api-sub-getallp", "api/v1/sub/fetchp/{i}", defaults: new { controller = "SubscriptionApi", action = "GetAllPaged" });

                     //routes.MapRoute("api-sub-delete", "api/v1/sub/delete/{id}", defaults: new { controller = "SubscriptionApi", action = "Delete" });

                     routes.MapRoute("api-sub-add", "api/v1/sub/add", defaults: new { controller = "SubscriptionApi", action = "AddAsync" });

                     // routes.MapRoute("api-sub-update", "api/v1/sub/update", defaults: new { controller = "SubscriptionApi", action = "Update" });

                     routes.MapRoute("api-sub-cancel", "api/v1/sub/cancel", defaults: new { controller = "SubscriptionApi", action = "Cancel" });

                     routes.MapRoute("api-sub-setseatcount", "api/v1/sub/setseatcount", defaults: new { controller = "SubscriptionApi", action = "SetSeatCount" });

                     routes.MapRoute("api-sub-sendinstructions", "api/v1/sub/sendinstructions", defaults: new { controller = "SubscriptionApi", action = "SendInstructions" });

                     routes.MapRoute("api-sub-auth", "api/v1/sub/auth", defaults: new { controller = "SubscriptionApi", action = "Auth" });

                     // Users

                     routes.MapRoute("api-user-getbyid", "api/v1/user/get/{id}", defaults: new { controller = "UserApi", action = "GetById" });

                     routes.MapRoute("api-user-getbyname", "api/v1/user/getfullname", defaults: new { controller = "UserApi", action = "GetFullName" });

                     routes.MapRoute("api-user-getall", "api/v1/user/fetch", defaults: new { controller = "UserApi", action = "GetAll" });

                     routes.MapRoute("api-user-getallp", "api/v1/user/fetchp", defaults: new { controller = "UserApi", action = "GetAllPaged" });

                     routes.MapRoute("api-user-add", "api/v1/user/add", defaults: new { controller = "UserApi", action = "Add" });

                     routes.MapRoute("api-user-delete", "api/v1/user/delete", defaults: new { controller = "UserApi", action = "Delete" });

                     routes.MapRoute("api-user-lock", "api/v1/user/lock", defaults: new { controller = "UserApi", action = "Lock" });

                     routes.MapRoute("api-user-unlock", "api/v1/user/unlock", defaults: new { controller = "UserApi", action = "UnLock" });

                     routes.MapRoute("api-user-verifyemail", "api/v1/user/verifyetoken", defaults: new { controller = "UserApi", action = "VerifyEmailToken" });

                     routes.MapRoute("api-user-sendreset", "api/v1/user/sendreset", defaults: new { controller = "UserApi", action = "SendResetPassword" });

                     routes.MapRoute("api-user-confirmreset", "api/v1/user/confirmreset", defaults: new { controller = "UserApi", action = "ConfirmResetPassword" });

                     routes.MapRoute("api-user-sendwelcome", "api/v1/user/sendwelcome", defaults: new { controller = "UserApi", action = "SendWelcomeEmail" });

                     // Lookup

                     routes.MapRoute("api-lookup-getallproducts", "api/v1/lookup/product/fetch", defaults: new { controller = "LookupApi", action = "GetAllProducts" });

                     routes.MapRoute("api-lookup-getalllicenceenviornments", "api/v1/lookup/le/fetch", defaults: new { controller = "LookupApi", action = "GetAllLicenceEnvironments" });

                     // Licence

                     routes.MapRoute("api-licence-getlicence", "api/v1/lic/getdetail", defaults: new { controller = "LicenceApi", action = "GetLicenceSummaryAsync" });

                     routes.MapRoute("api-licence-deseat", "api/v1/lic/deseat", defaults: new { controller = "LicenceApi", action = "DeactivateSeatAsync" });

                     //Dashboard

                     routes.MapRoute("api-dashboard-get", "api/v1/dashboard/get", defaults: new { controller = "DashboardApi", action = "GetAll" });

                     //UserData

                     routes.MapRoute("api-userdata-add", "api/v1/userdata/add", defaults: new { controller = "UserDataApi", action = "Add" });

                     routes.MapRoute("api-userdata-fetch", "api/v1/userdata/fetch", defaults: new { controller = "UserDataApi", action = "GetAll" });

                     //Report

                     routes.MapRoute("api-report-fetch", "api/v1/report/fetch/{s}", defaults: new { controller = "ReportApi", action = "Fetch" });

                     routes.MapRoute("api-report-fetchbydate", "api/v1/report/fetch/{s}/{d}", defaults: new { controller = "ReportApi", action = "FetchByDate" });

                     routes.MapRoute("api-report-fetchp", "api/v1/report/fetchp", defaults: new { controller = "ReportApi", action = "FetchPaged" });

                     routes.MapRoute("api-report-start", "api/v1/report/start", defaults: new { controller = "ReportApi", action = "TriggerReportService" });

                     routes.MapRoute("api-report-getruns", "api/v1/report/getruns", defaults: new { controller = "ReportApi", action = "GetReportProcessorRuns" });
                     // Identity

                     routes.MapRoute("api-memberhsip-createuser", "api/v1/identity/adduser", defaults: new { controller = "IdentityApi", action = "CreateUser" });

                     routes.MapRoute("api-memberhsip-createrole", "api/v1/identity/addrole", defaults: new { controller = "IdentityApi", action = "CreateRole" });

                     routes.MapRoute("api-memberhsip-assignrole", "api/v1/identity/assignrole", defaults: new { controller = "IdentityApi", action = "AssignUserToRole" });

                     // Token

                     routes.MapRoute("api-token-createuser", "api/v1/token/get", defaults: new { controller = "TokenApi", action = "Get" });

                     routes.MapRoute("api-token-regenerate", "api/v1/token/regenerate", defaults: new { controller = "TokenApi", action = "RegenerateAsync" });


                     // Configuration

                     routes.MapRoute("api-cfg-getpagesize", "api/v1/cfg/pagesize", defaults: new { controller = "ConfigurationApi", action = "GetPageSize" });

                     // Seat 

                     routes.MapRoute("api-seat-fetchp", "api/v1/seat/fetchp", defaults: new { controller = "SeatApi", action = "GetAllPaged" });

                     // ekey 

                     routes.MapRoute("api-ekey-getkey", "api/v1/ekey/getkey/{subId}", defaults: new { controller = "EKeyApi", action = "GetKey" });
                     routes.MapRoute("api-ekey-testencrypt", "api/v1/ekey/testencrypt", defaults: new { controller = "EKeyApi", action = "TestEncrypt" });
                     routes.MapRoute("api-ekey-testdecrypt", "api/v1/ekey/testdecrypt", defaults: new { controller = "EKeyApi", action = "TestDecrypt" });


                     //Device Events
                     routes.MapRoute("api-device-info", "api/v1/device/info", defaults: new { controller = "DeviceApi", action = "DeviceInformationEventsAsync" });
                     routes.MapRoute("api-device-webprotect", "api/v1/device/webprotect", defaults: new { controller = "DeviceApi", action = "WebProtectEventAsync" });
                     routes.MapRoute("api-device-health", "api/v1/device/health", defaults: new { controller = "DeviceApi", action = "DeviceHealthEventAsync" });
                     routes.MapRoute("api-device-scansummary", "api/v1/device/scansummary", defaults: new { controller = "DeviceApi", action = "ScanSummaryEventAsync" });
                     routes.MapRoute("api-device-malwaredetect", "api/v1/device/malwaredetect", defaults: new { controller = "DeviceApi", action = "MalwareDetectionEventAsync" });
                     routes.MapRoute("api-device-secureapps", "api/v1/device/secureapps", defaults: new { controller = "DeviceApi", action = "SecureAppsEventAsync" });
                     routes.MapRoute("api-device-malwareremediate", "api/v1/device/malwareremediate", defaults: new { controller = "DeviceApi", action = "MalwareRemediationEventAsync" });
                     routes.MapRoute("api-device-realtimeprotect", "api/v1/device/realtimeprotect", defaults: new { controller = "DeviceApi", action = "RealtimeProtectionEventAsync" });
                     routes.MapRoute("api-device-firewallevent", "api/v1/device/firewallevent", defaults: new { controller = "DeviceApi", action = "FirewallEventAsync" });
                     routes.MapRoute("api-device-firewallpolicy", "api/v1/device/firewallpolicy", defaults: new { controller = "DeviceApi", action = "FirewallPolicyEventAsync" });
                 }
            );

        }
    }
}
