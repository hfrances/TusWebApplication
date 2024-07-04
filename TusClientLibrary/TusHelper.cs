using TusDotNetClientSync = qckdev.Storage.TusDotNetClientSync;
using System;
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

    }
}
