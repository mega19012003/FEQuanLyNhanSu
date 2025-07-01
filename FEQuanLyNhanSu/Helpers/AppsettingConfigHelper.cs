using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FEQuanLyNhanSu.Helpers
{
    public static class AppsettingConfigHelper
    {
        public static IConfigurationRoot Configuration { get; private set; }

        static AppsettingConfigHelper()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static string GetBaseUrl()
        {
            return Configuration["ApiBaseUrl"];
        }
    }
}
