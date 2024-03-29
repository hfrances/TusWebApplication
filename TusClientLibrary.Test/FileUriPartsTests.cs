using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary.Test
{

    [TestClass]
    public sealed class FileUriPartsTests
    {

        [TestMethod]
        public void ParseUri_Format1()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse(new Uri("https://localhost:5120/storage"), new Uri("files/mystore/other/prueba", UriKind.Relative));
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage"), "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

        [TestMethod]
        public void ParseUri_Format2()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse(new Uri("https://localhost:5120/storage"), new Uri("/files/mystore/other/prueba", UriKind.Relative));
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage"), "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

        [TestMethod]
        public void ParseUri_Format3()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse(new Uri("https://localhost:5120/storage/"), new Uri("files/mystore/other/prueba", UriKind.Relative));
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage/"), "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

        [TestMethod]
        public void ParseString_Absolute()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("https://localhost:5120/storage/files/mystore/other/prueba");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage"), "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

        [TestMethod]
        public void ParseString_Absolute_Version()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("https://localhost:5120/storage/files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage"), "mystore", "other", "prueba", "other/prueba", "2022-11-14T14:55:05.2489168Z")
            );
            Assert.AreEqual(
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z",
                parts.GetRelativeUrl(withVersion: true)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.GetRelativeUrl(withVersion: false)
            );
        }

        [TestMethod]
        public void ParseString_Relative_Format1()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("files/mystore/other/prueba");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                ((Uri)null, "mystore", "other", "prueba", "other/prueba", (string)null)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.GetRelativeUrl()
            );
        }

        [TestMethod]
        public void ParseString_Relative_Format1_Version()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                ((Uri)null, "mystore", "other", "prueba", "other/prueba", "2022-11-14T14:55:05.2489168Z")
            );
            Assert.AreEqual(
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z",
                parts.GetRelativeUrl(withVersion: true)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.GetRelativeUrl(withVersion: false)
            );
        }

        [TestMethod]
        public void ParseString_Relative_Format2()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("/files/mystore/other/prueba");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                ((Uri)null, "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

    }
}
