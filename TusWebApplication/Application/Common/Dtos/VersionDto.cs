using System;

namespace TusWebApplication.Application.Common.Dtos
{
    public sealed class VersionDto
    {
        public Guid? ProductId { get; internal set; }
        public string? Version { get; internal set; }
        public string? OsPlatform { get; internal set; }
        public string? TargetFramework { get; internal set; }
    }
}
