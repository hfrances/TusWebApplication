using qckdev.AspNetCore.Authentication.JwtBearer;
using System;

namespace TusWebApplication.TusAzure.Authentication
{
    sealed class JwtBearerMoreOptions : qckdev.AspNetCore.Authentication.JwtBearer.JwtBearerMoreOptions
    {

        /// <summary>
        /// Gets or sets limited time (in seconds) for stating to upload the file.
        /// </summary>
        public TimeSpan? FirstRequestLifeTimeSpan { get; set; }

    }
}
