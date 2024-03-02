using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    static class HttpUtility
    {

        public static IDictionary<string, string> ParseQueryString(string query)
        {
            IDictionary<string, string> result;
            string[] queryParams;

            if (query == null)
            {
                result = null;
            }
            else
            {
                if (query.StartsWith("?") || query.StartsWith("#"))
                {
                    query = query.Substring(1);
                }
                queryParams = query.Split('&');
                result = new Dictionary<string, string>();
                foreach (string param in queryParams)
                {
                    string[] keyValue = param.Split('=');

                    if (keyValue.Length == 2)
                    {
                        var key = Uri.UnescapeDataString(keyValue[0]);
                        var value = Uri.UnescapeDataString(keyValue[1]);

                        result.Add(key, value);
                    }
                }
            }
            return result;
        }

        public static string BuildQueryString(IDictionary<string, string> queryParameters)
        {
            var keyValuePairs = new List<string>();
            foreach (var parameter in queryParameters)
            {
                string key = Uri.EscapeDataString(parameter.Key);
                string value = (parameter.Value == null) ? null : Uri.EscapeDataString(parameter.Value);
                keyValuePairs.Add($"{key}={value}");
            }
            return string.Join("&", keyValuePairs);
        }

    }
}
