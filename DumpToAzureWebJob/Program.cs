using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using Flexerant.MongoDataIO.Core;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text;

namespace DumpToAzureWebJob
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var databaseName = args[0];
            var slackChannel = Environment.GetEnvironmentVariable("SLACK_CHANNEL_WEBHOOK");
            //var logFile = args[1];
            var processOutput = new StringBuilder();

            void writeToLog(string message)
            {
                Console.WriteLine(message);
                processOutput.AppendLine(message);
            }

            writeToLog($"Running backup job for {databaseName} database.");
            //writeToLog($"Writing results to {logFile}.");

            var log = new DumpLog() {  DatabaseName = databaseName };
            BlobContainerClient? container = null;
                        
            try
            {
                var target = new AzureTarget(GetMongoBinFolder().FullName);
                var clusertUri = Environment.GetEnvironmentVariable("MONGO_CLUSTER_URI");
                var azureBlobConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
                var dumpResult = target.DumpToAzure(clusertUri, databaseName, azureBlobConnectionString, writeToLog);

                container = new BlobContainerClient(azureBlobConnectionString, dumpResult.ContainerName);
                log.Success = true;
                log.Message = processOutput.ToString();

                return 0;
            }
            catch (Exception ex)
            {
                log.Success = false;
                log.Message = ex.Message;

                writeToLog(ex.Message);

                return 1;
            }
            finally
            {
                Slack.SendMessageToSlack(log, slackChannel);
            }
        }

        static DirectoryInfo GetMongoBinFolder()
        {
            return new DirectoryInfo(Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, @"mongodb_db_tools", "bin"));
        }
    }
}