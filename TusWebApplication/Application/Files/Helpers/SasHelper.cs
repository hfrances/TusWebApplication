using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TusWebApplication.Application.Files.Helpers
{
    static class SasHelper
    {

        static readonly SHA256 ShaHash = SHA256.Create();


        public static string GenerateSasString(DateTimeOffset expiresOn, BlobClient blob, BlobProperties properties, string? versionId)
        {
            var token = GenerateSasHash(expiresOn, blob, properties);
            var query = new Dictionary<string, string?>();
            string queryString;

            if (versionId != null)
            {
                query.Add("versionId", versionId);
            }
            query.Add("sv", "1");
            query.Add("se", expiresOn.ToString("O"));
            query.Add("sig", token);
            queryString = QueryHelpers.AddQueryString("", query);
            return queryString.Substring(1); // Remove "?"
        }

        public static string GenerateSasHash(DateTimeOffset expiresOn, BlobClient blob, BlobProperties properties)
        {
            var builder = new StringBuilder();

            builder.Append(blob.BlobContainerName);
            builder.Append(blob.Name);
            builder.Append(properties.CreatedOn.ToString("s"));
            builder.Append(properties.ETag);
            builder.Append(properties.VersionId);
            builder.Append(expiresOn.ToString("s"));

            return GetSha256Hash(ShaHash, builder.ToString());
        }

        public static void ValidateSasHash(string? sasVersion, DateTimeOffset? expiresOn, string? sig, BlobClient blob, BlobProperties properties, bool useSas)
        {

            if (sasVersion != null || expiresOn != null || sig != null)
            {
                if (sasVersion == null || expiresOn == null || sig == null)
                {
                    throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.BadRequest, "Missed SAS parameters.");
                }
                else
                {
                    switch (sasVersion)
                    {
                        case "1":
                            var internalSig = GenerateSasHash(expiresOn.Value, blob, properties);

                            if (internalSig != sig)
                            {
                                throw new Exceptions.InvalidSasTokenException("Signature did not match.");
                            }
                            break;
                        default:
                            throw new Exceptions.InvalidSasTokenException("Invalid signature version.");
                    }

                    if (expiresOn < DateTimeOffset.UtcNow)
                    {
                        throw new Exceptions.InvalidSasTokenException("Access expired.");
                    }
                }
            }
            else if (useSas)
            {
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.NotFound, "The specified resource does not exist.");
            }
        }

        static string GetSha256Hash(SHA256 shaHash, string input)
        {
            return Convert.ToBase64String(shaHash.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

    }
}
