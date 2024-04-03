namespace TusWebApplication.Settings
{
    sealed class JwtTokenConfiguration
    {

        public string Key { get; set; } = string.Empty;
        public long AccessExpireSeconds { get; set; }

    }
}
