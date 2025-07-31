using System;
using System.Net;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace TusWebApplication.AzureExceptions
{
    static class AzureExeptionHelper
    {

        // https://regex101.com/r/c0VaZC/2
        readonly static Regex MacRegex = new Regex(
            @"(?<fragment1>The MAC signature found in the HTTP request).*?(?<fragment2>is not the same as any computed signature)\..*?restype:(?<restype>[^\s']+)",
            RegexOptions.Singleline
        );
        // https://regex101.com/r/1IWkSF/1
        readonly static Regex TagRegex = new Regex(
            @"(?<message>The tags specified are invalid\. It contains characters that are not permitted\.)[\s\S]*?Additional Information:\s*(?<tagvalues>(?:TagValue:\s*[^\r\n]+\s*)+)\s",
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
            else if (ex.ErrorCode == "InvalidTag")
            {
                messageId = "error.azureInvalidTag";
                var match = TagRegex.Match(ex.Message);
                if (match.Success)
                {
                    messsge = $"{match.Groups["message"]}\nAdditional Information:\n{match.Groups["tagvalues"]}".TrimEnd();
                    exception = new qckdev.AspNetCore.HttpHandledException(code, messsge, ex);
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
