using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary.Test.Base
{
    public abstract class BaseTests : IDisposable
    {

        const int PORT = 5120;

        protected static IWebHost Factory { get; private set; } = null!;
        protected internal static Settings.Configuration Settings { get; private set; } = null!;
        protected HttpClient HttpClient { get; private set; } = null!;

        public BaseTests()
        {
            Factory =
                new WebHostBuilder()
                    .ConfigureAppConfiguration((_, configurationBuilder) =>
                        configurationBuilder.AddJsonFile($"appsettings.Testing.json", true, true))
                    .UseKestrel(x => x.ListenLocalhost(PORT))
                    .UseStartup<TusWebApplication.Startup>()
                    .Build()
            ;
            Factory.Start();
            Settings = TestHelper.GetSettings("Testing");
        }

        [TestInitialize]
        public void TestInit()
        {
            HttpClient = new HttpClient() { BaseAddress = new UriBuilder("http", "localhost", PORT, "/storage/").Uri };
        }

        public void Dispose()
        {
            Factory.StopAsync().Wait();
            Factory?.Dispose();
        }
    }
}
