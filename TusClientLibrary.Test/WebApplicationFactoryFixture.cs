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
    public class WebApplicationFactoryFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        public string HostUrl { get; set; } = "http://localhost:5120"; // we can use any free port

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(HostUrl);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var dummyHost = builder.Build();
            dummyHost.Start();

            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration((_, configurationBuilder) =>
                            configurationBuilder.AddJsonFile($"appsettings.Testing.json", true, true))
                        .UseUrls(HostUrl)
                        .UseKestrel()
                        .UseStartup<TEntryPoint>();
                })
                .Build()
            ;
            host.Start();

            //var builder2 = builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());
            //var host = builder2.Build();
            //host.Start();

            return dummyHost;
        }

    }
}
