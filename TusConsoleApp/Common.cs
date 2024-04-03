using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            return MimeMapping.MimeUtility.GetMimeMapping(fileName);
        }

    }
}
