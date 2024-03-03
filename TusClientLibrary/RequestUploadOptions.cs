using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public class RequestUploadOptions
    {

        /// <summary>
        /// The MIME content type of the blob.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Specifies the natural languages used by this resource.
        /// </summary>
        public string ContentLanguage { get; set; }

        /// <summary>
        /// When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="TusClient.GetFileDetails(string)"./>
        /// </summary>
        public bool UseQueueAsync { get; set; }

        public string Hash { get; set; }

    }
}
