using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TusClientLibrary
{

    /// <remarks>
    /// https://learn.microsoft.com/es-es/dotnet/framework/network-programming/tls
    /// https://learn.microsoft.com/es-es/windows/win32/secauthn/protocols-in-tls-ssl--schannel-ssp-#tls-protocol-version-support
    /// </remarks>
    static class SslProtocolsExtensions
    {
        // For .NET Framework 3.5
        public const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;
        // For .NET Framework 4.6.2 and later
        public const SecurityProtocolType Tls13 = (SecurityProtocolType)12288;
    }
}
