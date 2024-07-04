using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TusClientLibrary
{
    public sealed class FileUriPartsComparer : IEqualityComparer<FileUriParts>
    {

        public static FileUriPartsComparer FullPath => new FileUriPartsComparer(FileUriPartsComparerType.FullPath);
        public static FileUriPartsComparer RelativePath => new FileUriPartsComparer(FileUriPartsComparerType.RelativePath);

        public FileUriPartsComparerType ComparerType { get; }


        public FileUriPartsComparer(FileUriPartsComparerType comparerType)
        {
            this.ComparerType = comparerType;
        }

        public bool Equals(FileUriParts x, FileUriParts y)
        {
            bool result;

            if (x == null && y == null)
            {
                result = true;
            }
            else if (x != null && x != null)
            {
                result = true;
                if (this.ComparerType == FileUriPartsComparerType.FullPath)
                {
                    result &= (x.BasePath == y.BasePath);
                }
                result &= (x.StoreName == y.StoreName);
                result &= (x.ContainerName == y.ContainerName);
                result &= (x.BlobName == y.BlobName);
                result &= (x.BlobId == y.BlobId);
                result &= (x.VersionId == y.VersionId);
            }
            else
            {
                result = false;
            }
            return result;
        }

        public int GetHashCode(FileUriParts obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            else
            {
                return 0;
                //return this.ComparerType switch
                //{
                //    FileUriPartsComparerType.FullPath =>
                //        HashCode.Combine(obj.BasePath, obj.StoreName, obj.ContainerName, obj.BlobName, obj.BlobId, obj.VersionId),
                //    _ =>
                //        HashCode.Combine(obj.StoreName, obj.ContainerName, obj.BlobName, obj.BlobId, obj.VersionId),
                //};
            }
        }

    }
}
