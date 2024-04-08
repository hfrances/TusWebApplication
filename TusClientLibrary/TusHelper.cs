using TusDotNetClient = qckdev.Storage.TusDotNetClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    static class TusHelper
    {

        public static void ApplyAuthorization(this TusDotNetClient.TusClient tusClient, string accessToken)
        {
            if (tusClient.AdditionalHeaders.ContainsKey("Authorization"))
            {
                tusClient.AdditionalHeaders.Remove("Authorization");
            }
            tusClient.AdditionalHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        public static (string key, string value)[] CreateMedatada(IDictionary<string, string> tags, IDictionary<string, string> metadata)
        {
            var metadataParsed = new List<(string key, string value)>();

            if (tags != null)
            {
                // tags
                foreach (var item in tags)
                {
                    metadataParsed.Add(($"TAG:{item.Key}", item.Value));
                }
            }
            if (metadata != null)
            {
                // metadata
                foreach (var item in metadata)
                {
                    metadataParsed.Add((item.Key, item.Value));
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
