using MediatR;
using TusWebApplication.Application.Files.Dtos;

namespace TusWebApplication.Application.Files.Commands
{

    public sealed class RequestUploadCommand : IRequest<RequestUploadDto>
    {

        public sealed class CommandBody
        {


            /// <summary>
            /// Name of the file.
            /// </summary>
            public string FileName { get; set; } = string.Empty;

            /// <summary>
            /// Name of the blob or null if name can be automatic.
            /// </summary>
            /// <remarks>
            /// See also <seealso cref="Replace"/> property.
            /// </remarks>
            public string? Blob { get; set; }

            /// <summary>
            /// If true, it will try to replace blob with id set in <see cref="Blob"/> property. 
            /// When false, if <see cref="Blob"/> property is not null and it already exists. It throws an exception when it tries to save the file.
            /// </summary>
            public bool Replace { get; set; }

            /// <summary>
            /// Size of the file that will be saved.
            /// </summary>
            public long Size { get; set; }

            /// <summary>
            /// Hash of the file. This value is optional and can be null.
            /// </summary>
            public string? Hash { get; set; }

            /// <summary>
            /// If true, it will upload file chunks without waiting that they are already saved in the azure blob. False otherwise.
            /// </summary>
            public bool UseQueueAsync { get; set; }
        }

        /// <summary>
        /// Store name
        /// </summary>
        public string StoreName { get; set; } = string.Empty;
        
        /// <summary>
        /// Container name
        /// </summary>
        public string Container { get; set; } = string.Empty;
               
        public CommandBody Body { get; set; } = new CommandBody();

    }

}
