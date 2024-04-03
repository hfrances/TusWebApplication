namespace TusWebApplication.TusAzure.Authentication
{
    sealed class JwtTokenConfiguration
    {

        /// <summary>
        /// Gets or sets the encryptation key for the uploading JWT token.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximun time (in seconds) for uploading the file fully.
        /// </summary>
        public long AccessExpireSeconds { get; set; }

        /// <summary>
        /// Gets or sets limited time (in seconds) for stating to upload the file.
        /// </summary>
        public long FirstRequestExpireSeconds { get; set; }

    }
}
