using TusDotNetClientSync = qckdev.Storage.TusDotNetClientSync;
using System;
using qckdev.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TusClientLibrary
{
    static class TusHelper
    {

        public static void ApplyAuthorization(this TusDotNetClientSync.TusClient tusClient, string accessToken)
        {
            if (tusClient.AdditionalHeaders.ContainsKey("Authorization"))
            {
                tusClient.AdditionalHeaders.Remove("Authorization");
            }
            tusClient.AdditionalHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        public static TusDotNetClientSync.TusMetadata[] CreateMedatada(IDictionary<string, string> tags, IDictionary<string, string> metadata)
        {
            var metadataParsed = new List<TusDotNetClientSync.TusMetadata>();

            if (tags != null)
            {
                // tags
                foreach (var item in tags)
                {
                    metadataParsed.Add(new TusDotNetClientSync.TusMetadata($"TAG:{item.Key}", item.Value));
                }
            }
            if (metadata != null)
            {
                // metadata
                foreach (var item in metadata)
                {
                    metadataParsed.Add(new TusDotNetClientSync.TusMetadata(item.Key, item.Value));
                }
            }

            return metadataParsed.ToArray();
        }

        public static TusResponse ParseResponse(string value)
        {
            TusResponse result;

            if (qckdev.Text.Json.JsonConvert.IsDeserializable(value))
            {
                result = qckdev.Text.Json.JsonConvert.DeserializeObject<TusResponse>(value);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public static Exceptions.TusHandledException CreateHandledException(
            FetchFailedException<TusResponse> exception,
            string storageName = null,
            string containerName = null,
            string blobName = null)
        {
            var errorCode = exception.Error?.Error?.Message;

            return CreateHandledException(errorCode, exception, storageName, containerName, blobName);
        }

        internal static Exceptions.TusHandledException CreateHandledException(
            string errorCode,
            Exception innerException,
            string storageName = null,
            string containerName = null,
            string blobName = null)
        {
            switch (errorCode)
            {
                case "error.BlobStorageNotFound":
                    return new Exceptions.BlobStorageNotFoundException(storageName, innerException);
                case "error.ContainerNotFound":
                    return new Exceptions.ContainerNotFoundException(storageName, containerName, innerException);
                case "error.BlobNotFound":
                    return new Exceptions.BlobNotFoundException(storageName, containerName, blobName, innerException);
                case "error.LoginFailed":
                    return new Exceptions.LoginException(errorCode, innerException);
                default:
                    return new Exceptions.TusHandledException(errorCode ?? innerException.Message, innerException);
            }
        }

    }
}
