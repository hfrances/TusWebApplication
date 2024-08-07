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
                parts.ToRelativeUrl(withVersion: true)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.ToRelativeUrl(withVersion: false)
            );
        }

        [TestMethod]
        public void ParseString_Absolute_Version_Sas()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("https://localhost:5120/storage/files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z&sv=1&se=2024-04-02T10%3A34%3A46.4108594%2B02%3A00&sig=wOKaVA2%2BmPv9Q9EMOp3ffT2Y8D1TDRiBD9XRJOhkVFM%3D");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage"), "mystore", "other", "prueba", "other/prueba", "2022-11-14T14:55:05.2489168Z")
            );
            Assert.AreEqual(
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z",
                parts.ToRelativeUrl(withVersion: true)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.ToRelativeUrl(withVersion: false)
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
                parts.ToRelativeUrl()
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
                parts.ToRelativeUrl(withVersion: true)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.ToRelativeUrl(withVersion: false)
            );
        }

        [TestMethod]
        public void ParseString_Relative_Format1_Version_Sas()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z&sv=1&se=2024-04-02T10%3A34%3A46.4108594%2B02%3A00&sig=wOKaVA2%2BmPv9Q9EMOp3ffT2Y8D1TDRiBD9XRJOhkVFM%3D");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                ((Uri)null, "mystore", "other", "prueba", "other/prueba", "2022-11-14T14:55:05.2489168Z")
            );
            Assert.AreEqual(
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z",
                parts.ToRelativeUrl(withVersion: true)
            );
            Assert.AreEqual(
                "files/mystore/other/prueba",
                parts.ToRelativeUrl(withVersion: false)
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

        [TestMethod]
        public void ParseString_Relative_StoreAndBlobId()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse("mystore", "other/prueba");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                ((Uri)null, "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

        [TestMethod]
        public void ParseString_Absolute_StoreAndBlobId()
        {
            FileUriParts parts;

            parts = FileUriParts.Parse(new Uri("https://localhost:5120/storage"), "mystore", "other/prueba");
            Assert.AreEqual(
                (parts.BasePath, parts.StoreName, parts.ContainerName, parts.BlobName, parts.BlobId, parts.VersionId),
                (new Uri("https://localhost:5120/storage"), "mystore", "other", "prueba", "other/prueba", (string)null)
            );
        }

        [TestMethod]
        public void ToRelativeUrl_Relative()
        {
            FileUriParts parts;

            parts = new FileUriParts { StoreName = "mystore", ContainerName = "other", BlobName = "prueba", VersionId = "2022-11-14T14:55:05.2489168Z" };
            Assert.AreEqual(
                parts.ToRelativeUrl(),
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z"
            );
        }

        [TestMethod]
        public void ToRelativeUrl_Absolute()
        {
            FileUriParts parts;

            parts = new FileUriParts { BasePath = new Uri("https://localhost:5120/storage"), StoreName = "mystore", ContainerName = "other", BlobName = "prueba", VersionId = "2022-11-14T14:55:05.2489168Z" };
            Assert.AreEqual(
                parts.ToRelativeUrl(),
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z"
            );
        }

        [TestMethod]
        public void ToAbsoluteUrl_Relative()
        {
            FileUriParts parts;

            parts = new FileUriParts { StoreName = "mystore", ContainerName = "other", BlobName = "prueba", VersionId = "2022-11-14T14:55:05.2489168Z" };
            Assert.ThrowsException<ArgumentNullException>(() =>
                parts.ToAbsoluteUrl()
            );
        }

        [TestMethod]
        public void ToAbsoluteUrl_Absolute()
        {
            FileUriParts parts;

            parts = new FileUriParts { BasePath = new Uri("https://localhost:5120/storage"), StoreName = "mystore", ContainerName = "other", BlobName = "prueba", VersionId = "2022-11-14T14:55:05.2489168Z" };
            Assert.AreEqual(
                parts.ToAbsoluteUrl(),
                "https://localhost:5120/files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z"
            );
        }

        [TestMethod]
        public void ToString_Relative()
        {
            FileUriParts parts;

            parts = new FileUriParts { StoreName = "mystore", ContainerName = "other", BlobName = "prueba", VersionId = "2022-11-14T14:55:05.2489168Z" };
            Assert.AreEqual(
                parts.ToString(),
                "files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z"
            );
        }

        [TestMethod]
        public void ToString_Absolute()
        {
            FileUriParts parts;

            parts = new FileUriParts { BasePath = new Uri("https://localhost:5120/storage"), StoreName = "mystore", ContainerName = "other", BlobName = "prueba", VersionId = "2022-11-14T14:55:05.2489168Z" };
            Assert.AreEqual(
                parts.ToString(),
                "https://localhost:5120/files/mystore/other/prueba?versionId=2022-11-14T14%3A55%3A05.2489168Z"
            );
        }

    }
}
