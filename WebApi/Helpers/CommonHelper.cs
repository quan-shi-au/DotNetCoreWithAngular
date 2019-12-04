using Microsoft.Extensions.Configuration;
using System.IO;

namespace ent.manager.WebApi.Helpers
{
    public class CommonHelper
    {
        
        public static IConfigurationRoot GetConfigurationObject()
        {
            var builder = new ConfigurationBuilder()
                    
                    .AddJsonFile("appsettings.json");

            var Configuration = builder.Build();

            return Configuration;

        }
   
    }
}
