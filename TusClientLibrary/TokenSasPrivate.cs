using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{
    sealed class TokenSasPrivate
    {

        public string StoreName { get; set; }

        public string ContainerName { get; set; }

        public string BlobName { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// Gets the token SAS for the blob file or null if the blob does not exist.
        /// </summary>
        public string TokenSas { get; set; }

    }
}
