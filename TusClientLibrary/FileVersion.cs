using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed class FileVersion
    {

        public string VersionId { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }

    }
}
