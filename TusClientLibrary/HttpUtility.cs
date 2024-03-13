using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{

    /// <summary>
    /// Provides methods for HTTP requests.
    /// </summary>
    public static class HttpUtility
    {

        /// <summary>
        /// Extracts elements from a uri query string.
        /// </summary>
        /// <param name="query">Value or the query string.</param>
        /// <returns>A dictionary with all elements in the query.</returns>
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

        /// <summary>
        /// Generates an uri query string from a dictionary.
        /// </summary>
        /// <param name="query">Dictionary with the keys and its values..</param>
        /// <returns>An string to use in a uri.</returns>
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

        /// <summary>
        /// Returs an <see cref="Uri"/> that is the same thant the original with the additional query values.
        /// </summary>
        /// <param name="uri">Original <see cref="Uri"/></param>
        /// <param name="values">A <see cref="IDictionary{string, string}"/> with the additinal query values.</param>
        /// <returns>The same thant the original with the additional query values.</returns>
        public static Uri WithQueryValues(this Uri uri, IDictionary<string, string> values)
        {
            var queryParameters = ParseQueryString(uri.Query);

            foreach (var pair in values)
            {
                queryParameters[pair.Key] = pair.Value;
            }
            return new UriBuilder(uri.GetLeftPart(UriPartial.Path))
            {
                Query = BuildQueryString(queryParameters)
            }.Uri;
        }

    }
}
