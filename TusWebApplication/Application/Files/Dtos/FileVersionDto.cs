using System;

namespace TusWebApplication.Application.Files.Dtos
{
    
    public sealed class FileVersionDto
    {

        public string VersionId { get; set; } = string.Empty;
        public DateTimeOffset? CreatedOn { get; set; }

    }

}
