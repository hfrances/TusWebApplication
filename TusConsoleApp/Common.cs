using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TusConsoleApp
{
    static class Common
    {

        public static string CalculateMD5(string fileName)
        {
            string contentHash;

            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(fileName))
                {
                    contentHash = Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
            return contentHash;
        }

        public static string CalculateMimeType(string fileName)
        {
#if NET461_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            return HeyRed.Mime.MimeGuesser.GuessMimeType(fileName);
#else
            return null;
#endif
        }

        public static bool? SetContentTypeAuto()
        {
#if NET461_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            return null;
#else
            return true;
#endif
        }

    }
}
