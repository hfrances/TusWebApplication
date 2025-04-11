using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace TusWebApplication.Common
{
    static class DependencyInjection
    {

        const string BASE_PATH_KEY = "BasePath";

        /// <summary>
        /// Loads the value from "BasePath" property set in appsettings file. It may have multiple values split by ";" character.
        /// </summary>
        public static IApplicationBuilder UsePathBase(this IApplicationBuilder app)
        {
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var basePathRaw = (configuration.GetSection(BASE_PATH_KEY)?.Value ?? "/");
            var basePaths = basePathRaw.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

            foreach (var basePath in basePaths)
            {
                app.UsePathBase(basePath);
            }
            return app;
        }

    }
}
