using Microsoft.AspNetCore.Cors.Infrastructure;
using System;

namespace TusWebApplication.Settings
{
    sealed class CorsSettings
    {

        /// <summary>
        /// Defines the policy for Cross-Origin requests based on the CORS specifications.
        /// </summary>
        public string[]? Origins { get; set; } = null;

        /// <summary>
        /// Gets the headers that the resource might use and can be exposed.
        /// </summary>
        public string[] ExposedHeaders { get; set; } = Array.Empty<string>();

    }
}
