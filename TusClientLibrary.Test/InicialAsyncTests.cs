using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TusClientLibrary.Test
{
    [TestClass]
    public sealed class InicialAsyncTests : Base.BaseTests
    {

        [TestMethod]
        public async Task GetVersionAsync()
        {
            var response = await HttpClient.GetAsync("/api/common/version");
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(response.IsSuccessStatusCode, responseString);
        }

        [TestMethod]
        public async Task AuthorizeAsync()
        {
            var response = await HttpClient.PostAsync("auth", JsonContent.Create(new
            {
                userName = Settings.Security.Credentials.UserName,
                login = Settings.Security.Credentials.Login,
                password = Settings.Security.Credentials.Password,
            }));
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(response.IsSuccessStatusCode, responseString);
        }

        [TestMethod]
        public async Task AuthorizeAsync_Fetch()
        {
            try
            {
                var response = await HttpClient.FetchAsync(HttpMethod.Post, "auth", new
                {
                    userName = Settings.Security.Credentials.UserName,
                    login = Settings.Security.Credentials.Login,
                    password = Settings.Security.Credentials.Password,
                });

                Assert.IsTrue(response != null);
            }
            catch (FetchFailedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}