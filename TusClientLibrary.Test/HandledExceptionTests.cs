using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TusClientLibrary.Exceptions;

namespace TusClientLibrary.Test
{

    [TestClass]
    public class HandledExceptionTests
    {

        const string STORAGE_NAME = "missing-storage";
        const string CONTAINER_NAME = "missing-container";
        const string BLOB_NAME = "missing-blob";

        [TestMethod]
        public void CreateHandledException_BlobStorageNotFound()
        {
            var exception = TusHelper.CreateHandledException(
                "error.BlobStorageNotFound", new Exception(),
                STORAGE_NAME, CONTAINER_NAME, BLOB_NAME);

            Assert.IsInstanceOfType(exception, typeof(BlobStorageNotFoundException));
            Assert.AreEqual(STORAGE_NAME, ((BlobStorageNotFoundException)exception).StorageName);
            StringAssert.Contains(exception.Message, STORAGE_NAME);
        }

        [TestMethod]
        public void CreateHandledException_ContainerNotFound()
        {
            var exception = TusHelper.CreateHandledException(
                "error.ContainerNotFound", new Exception(),
                STORAGE_NAME, CONTAINER_NAME, BLOB_NAME);

            Assert.IsInstanceOfType(exception, typeof(ContainerNotFoundException));
            var typedException = (ContainerNotFoundException)exception;
            Assert.AreEqual(STORAGE_NAME, typedException.StorageName);
            Assert.AreEqual(CONTAINER_NAME, typedException.ContainerName);
            StringAssert.Contains(exception.Message, STORAGE_NAME);
            StringAssert.Contains(exception.Message, CONTAINER_NAME);
        }

        [TestMethod]
        public void CreateHandledException_BlobNotFound()
        {
            var exception = TusHelper.CreateHandledException(
                "error.BlobNotFound", new Exception(),
                STORAGE_NAME, CONTAINER_NAME, BLOB_NAME);

            Assert.IsInstanceOfType(exception, typeof(BlobNotFoundException));
            var typedException = (BlobNotFoundException)exception;
            Assert.AreEqual(STORAGE_NAME, typedException.StorageName);
            Assert.AreEqual(CONTAINER_NAME, typedException.ContainerName);
            Assert.AreEqual(BLOB_NAME, typedException.BlobName);
            StringAssert.Contains(exception.Message, STORAGE_NAME);
            StringAssert.Contains(exception.Message, CONTAINER_NAME);
            StringAssert.Contains(exception.Message, BLOB_NAME);
        }

        [TestMethod]
        public void CreateHandledException_LoginFailed()
        {
            var exception = TusHelper.CreateHandledException(
                "error.LoginFailed", new Exception());

            Assert.IsInstanceOfType(exception, typeof(LoginException));
        }

        [TestMethod]
        public void CreateHandledException_UnknownError()
        {
            const string errorCode = "error.Unknown";

            var exception = TusHelper.CreateHandledException(
                errorCode, new Exception());

            Assert.AreEqual(typeof(TusHandledException), exception.GetType());
            Assert.AreEqual(errorCode, exception.Message);
        }

    }
}
