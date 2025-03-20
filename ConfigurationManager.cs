/*using Microsoft.Extensions.Configuration;
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
        public static string GetDbPath()
        {
            return Configuration["DatabaseSettings:DbPath"];
        }
    }
}
*/

using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SystemMonitor
{
    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; }

        static ConfigurationManager()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public static string GetBaseUrl() => Configuration["ApiConfig:BaseUrl"];
        public static string GetBaseUrlForApps() => Configuration["ApiConfig:BaseUrlForApps"];
        public static string GetDbPath() => Configuration["DatabaseSettings:DbPath"];
        public static string GetSocketServerUrl() => Configuration["SocketSettings:ServerUrl"];
        public static string GetInstallerApiUrl() => Configuration["SocketSettings:InstallerApiUrl"];
    }
}
