using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{
    public sealed class CreateFileOptions : RequestUploadOptions
    {

        /// <summary>
        /// A list of tags added to the blob. Tags can be used for filtering in the blob.
        /// </summary>
        public IDictionary<string, string> Tags { get; set; }
        /// <summary>
        /// A list of metadatas added to the blob.
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }

    }
}
