using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{
    public class HttpRequestMethod
    {

        public static HttpRequestMethod Get { get; } = new HttpRequestMethod("GET");
        public static HttpRequestMethod Post { get; } = new HttpRequestMethod("POST");
        public static HttpRequestMethod Put { get; } = new HttpRequestMethod("PUT");
        public static HttpRequestMethod Delete { get; } = new HttpRequestMethod("DELETE");
        public static HttpRequestMethod Head { get; } = new HttpRequestMethod("HEAD");
        public static HttpRequestMethod Options { get; } = new HttpRequestMethod("OPTIONS");
        public static HttpRequestMethod Path { get; } = new HttpRequestMethod("PATH");
        public static HttpRequestMethod Trace { get; } = new HttpRequestMethod("TRACE");

        protected HttpRequestMethod(string method)
        {
            this.Method = method;
        }

        public string Method { get; }

    }
}
