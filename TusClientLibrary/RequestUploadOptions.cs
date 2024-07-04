using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{
    public class RequestUploadOptions
    {

        /// <summary>
        /// The MIME content type of the blob.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// When true and <see cref="ContentType"/> is empty, calculates the MIME content type automaticaly.
        /// </summary>
        public bool? ContentTypeAuto { get; set; }

        /// <summary>
        /// Specifies the natural languages used by this resource.
        /// </summary>
        public string ContentLanguage { get; set; }

        /// <summary>
        /// When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="TusClient.GetFileDetails(string, bool)"/>.
        /// </summary>
        public bool UseQueueAsync { get; set; }

        /// <summary>
        /// A MD5 value that must checks with the file uploaded for making sure the file integrity. Null for not checking.
        /// </summary>
        public string Hash { get; set; }

    }
}
