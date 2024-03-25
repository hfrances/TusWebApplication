using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary.Test
{
    [TestClass]
    public sealed class FileUriPartsComparerTests
    {

        [TestMethod]
        public void Compare_FullPath()
        {
            var actual = FileUriParts.Parse("http://mydomain.com/mybasepath/files/mystore/mycontainer/myfile");
            var expected = new FileUriParts
            {
                BasePath = new Uri("http://mydomain.com/mybasepath"),
                StoreName = "mystore",
                ContainerName = "mycontainer",
                BlobName = "myfile"
            };

            Assert.IsTrue(FileUriPartsComparer.FullPath.Equals(actual, expected));
        }

        [TestMethod]
        public void Compare_RelativePath_Format1()
        {
            var actual = FileUriParts.Parse("http://mydomain.com/mybasepath/files/mystore/mycontainer/myfile");
            var expected = new FileUriParts
            {
                BasePath = new Uri("http://achilipu.com/mybasepath/"),
                StoreName = "mystore",
                ContainerName = "mycontainer",
                BlobName = "myfile"
            };

            Assert.IsTrue(FileUriPartsComparer.RelativePath.Equals(actual, expected));
        }

        [TestMethod]
        public void Compare_RelativePath_Format2()
        {
            var actual = FileUriParts.Parse("files/mystore/mycontainer/myfile");
            var expected = new FileUriParts
            {
                BasePath = new Uri("http://achilipu.com/mybasepath/"),
                StoreName = "mystore",
                ContainerName = "mycontainer",
                BlobName = "myfile"
            };

            Assert.IsTrue(FileUriPartsComparer.RelativePath.Equals(actual, expected));
        }

    }
}
