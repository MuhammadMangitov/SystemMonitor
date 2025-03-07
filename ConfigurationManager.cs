using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SystemMonitor
{
    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; }

        static ConfigurationManager()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }
        //aaa
        public static string GetDbPath()
        {
            return Configuration["DatabaseSettings:DbPath"];
        }
    }
}
