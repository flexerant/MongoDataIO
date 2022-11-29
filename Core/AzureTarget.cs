using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;

namespace Flexerant.MongoDataIO.Core
{
    public class AzureTarget
    {
        private readonly FileInfo _mongodumpExe;
        private readonly FileInfo _mongorestoreExe;

        public AzureTarget(string mongoBinFolder)
        {
            _mongodumpExe = new FileInfo(Path.Combine(mongoBinFolder, "mongodump.exe"));
            _mongorestoreExe = new FileInfo(Path.Combine(mongoBinFolder, "mongorestore.exe"));
        }

        public AzureTarget(DirectoryInfo mongoBinFolder) : this(mongoBinFolder.FullName) { }

        public DumpToAzureResult DumpToAzure(string mongoConnectionString, string databaseName, string blobConnectionString, Action<string> outputData = null)
        {
            DateTime now = DateTime.UtcNow;
            string containerName = databaseName.ToLower();

            return this.DumpToAzure(mongoConnectionString, databaseName, blobConnectionString, containerName, outputData);
        }

        public DumpToAzureResult DumpToAzure(string mongoConnectionString, string databaseName, string blobConnectionString, string containerName, Action<string> outputData = null)
        {
            StreamTarget streamTarget = new StreamTarget(_mongodumpExe.Directory);
            FileTarget fileTarget = new FileTarget(_mongodumpExe.Directory);
            DateTime now = DateTime.UtcNow;
            string blobName = $"{now.ToString("s").Replace(':', '-')}_{databaseName}.bak";
            BlobContainerClient container = new BlobContainerClient(blobConnectionString, containerName);
            BlockBlobClient client = container.GetBlockBlobClient(blobName);
            List<string> logData = new List<string>();

            container.CreateIfNotExists();

            FileInfo tempFile = null;

            try
            {
                tempFile = fileTarget.DumpToFile(mongoConnectionString, databaseName, outputData);

                using (FileStream fs = new FileStream(tempFile.FullName, FileMode.Open, FileAccess.Read))
                {
                    var blob = client.Upload(fs, new Azure.Storage.Blobs.Models.BlobUploadOptions());                    
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    if (tempFile != null)
                    {
                        tempFile.Delete();
                    }
                }
                catch { }
            }

            return new DumpToAzureResult() { BlobName = blobName, ContainerName = containerName, LogData = logData };
        }

        public void RestoreFromAzure(
            string mongoConnectionString,
            string blobConnectionString,
            string blobContainer,
            string blobName,
            string sourceDatabaseName,
            string destinationDatabaseName,
            Action<string> outputData = null)
        {
            StreamTarget streamTarget = new StreamTarget(_mongorestoreExe.Directory);
            BlobContainerClient container = new BlobContainerClient(blobConnectionString, blobContainer);
            BlobClient client = container.GetBlobClient(blobName);

            using (Stream downloadStream = client.Download().Value.Content)
            {
                streamTarget.RestoreFromStream(downloadStream, mongoConnectionString, sourceDatabaseName, destinationDatabaseName, outputData);
            }
        }
    }
}
