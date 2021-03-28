using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CliWrap;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        public DumpToAzureResult DumpToAzure(string clusterUri, string databaseName, string blobConnectionString, Action<string> outputData = null)
        {
            string uri = Helpers.FormatUri(clusterUri);
            DateTime now = DateTime.UtcNow;
            string containerName = databaseName.ToLower();
            string blobName = $"{now.ToString("s").Replace(':', '-')}_{databaseName}.bak";
            BlobContainerClient container = new BlobContainerClient(blobConnectionString, containerName);
            BlockBlobClient client = container.GetBlockBlobClient(blobName);
            CommandResult result;
            StringBuilder sb = new StringBuilder();

            container.CreateIfNotExists();

            using (var blobStream = client.OpenWrite(true))
            {
                var cmd = Cli.Wrap(_mongodumpExe.FullName)
                    .WithArguments($"--uri {uri}/{databaseName} --archive")
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(text =>
                    {
                        sb.AppendLine(text);
                        outputData?.Invoke(text);
                    }, Encoding.ASCII))
                    .WithStandardOutputPipe(PipeTarget.ToStream(blobStream))
                    .WithValidation(CommandResultValidation.None);

                result = Task.Run(async () => await cmd.ExecuteAsync()).Result;
            }

            if (result.ExitCode != 0)
            {
                throw new Exception(sb.ToString());
            }

            return new DumpToAzureResult() { BlobName = blobName, ContainerName = containerName };
        }

        public void RestoreFromAzure(string clusterUri, string blobConnectionString, string blobContainer, string blobName, Action<string> outputData = null)
        {
            string uri = Helpers.FormatUri(clusterUri);
            BlobContainerClient container = new BlobContainerClient(blobConnectionString, blobContainer);
            BlobClient client = container.GetBlobClient(blobName);
            string nsFrom = $"{blobContainer}.*";
            string nsTo = $"{blobContainer}-restore.*";

            using (Stream downloadStream = client.Download().Value.Content)
            {
                var cmd = Cli.Wrap(_mongorestoreExe.FullName)
               .WithArguments($"--uri {uri} --archive --nsFrom \"{nsFrom}\" --nsTo \"{nsTo}\"")
               .WithStandardInputPipe(PipeSource.FromStream(downloadStream))
               .WithStandardErrorPipe(PipeTarget.ToDelegate(text => outputData?.Invoke(text), Encoding.ASCII));

                Task.Run(async () => await cmd.ExecuteAsync()).Wait();
            }
        }
    }
}
