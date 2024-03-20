using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary.Test
{
    public class WebApplicationFactoryFixture2<TEntryPoint> : WebHostBuilder
        where TEntryPoint : class
    {

        public string HostUrl { get; set; }

        public WebApplicationFactoryFixture2(string hostUrl, string environment = "Test")
        {
            this.HostUrl = hostUrl;
            this
                .ConfigureAppConfiguration((_, configurationBuilder) =>
                            configurationBuilder.AddJsonFile($"appsettings.{environment}.json", true, true))
                .UseUrls(HostUrl)
                .UseKestrel()
                .UseStartup<TEntryPoint>();
        }



    }
}
