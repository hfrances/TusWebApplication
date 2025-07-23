using System;
using System.Net;
using System.Text.RegularExpressions;

namespace TusWebApplication.Application.Files.Helpers
{
    static class ExeptionHelper
    {

        // https://regex101.com/r/c0VaZC/2
        readonly static Regex MacRegex = new Regex(
            @"(?<fragment1>The MAC signature found in the HTTP request).*?(?<fragment2>is not the same as any computed signature)\..*?restype:(?<restype>[^\s']+)",
            RegexOptions.Singleline
        );

        public static qckdev.AspNetCore.HttpHandledException CreateException(Azure.RequestFailedException ex)
        {
            qckdev.AspNetCore.HttpHandledException? exception = null;
            string? messageId = null;
            string? messsge = null;
            HttpStatusCode code = (HttpStatusCode)ex.Status;

            if (ex.ErrorCode == "AuthenticationFailed")
            {
                messageId = "error.azureAuthenticationFailed";
                if (ex.Data.Count > 0)
                {
                    var text = (string)(ex.Data["AuthenticationErrorDetail"] ?? string.Empty);
                    var match = MacRegex.Match(text);

                    if (match.Success)
                    {
                        messsge = $"{match.Groups["fragment1"]} {match.Groups["fragment2"]} (restype: {match.Groups["restype"]}).";
                        exception = new qckdev.AspNetCore.HttpHandledException(code, messsge, ex);
                    }
                }
                exception = new qckdev.AspNetCore.HttpHandledException(code, messageId, exception);
            }

            if (exception == null)
            {
                exception = new qckdev.AspNetCore.HttpHandledException(code, ex.Message, ex);
            }
            return exception;
        }

    }
}
