using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{
    public sealed class TokenSas
    {

        /// <summary>
        /// Gets the token SAS for the blob file or null if the blob does not exist.
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Gets the token SAS for the blob file or null if the blob does not exist.
        /// </summary>
        public string RelativeUrl { get; internal set; }

    }
}
