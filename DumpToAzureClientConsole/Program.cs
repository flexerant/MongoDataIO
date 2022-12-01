using CommandLine;
using RestSharp.Authenticators;
using RestSharp;
using System.Globalization;
using DumpToAzureWebJob;
using Azure.Storage.Blobs;
using System.ComponentModel;
using System.Diagnostics;

namespace DumpToAzureClientConsole
{
    internal class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                   (Options opts) => Dump(opts),
                   errs => 1);
        }

        static int Dump(Options options)
        {
            try
            {
                var logFile = $"{Guid.NewGuid()}.json";

                var client = new RestClient(options.WebjobUrl)
                {
                    Authenticator = new HttpBasicAuthenticator(options.Username, options.Password)
                };

                var request = new RestRequest() { Method = Method.Post };

                request.AddQueryParameter("arguments", $"{options.DatabaseName} {logFile}");

                var response = client.Execute(request);

                if (!response.IsSuccessful)
                {
                    var message = !string.IsNullOrWhiteSpace(response.ErrorMessage) ? response.ErrorMessage : response.Content;

                    throw new Exception("An error occured when calling the webjob.", new Exception(message));
                }

                GetLogMessage(logFile, options);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return 1;
            }
        }

        static void GetLogMessage(string logFile, Options options)
        {            
            var maxDuration = TimeSpan.FromMinutes(options.RetryDurationMinutes);
            var timer = new Stopwatch();

            timer.Start();

            var containerClient = new BlobContainerClient(options.BlobConnectionString, options.DatabaseName);
            var blobClient = containerClient.GetBlobClient(logFile);
            var exists = blobClient.Exists();

            while (!exists)
            {
                if (timer.Elapsed.TotalSeconds > maxDuration.TotalSeconds)
                {
                    throw new Exception("Number of retries exceeded.");
                }

                Thread.Sleep(5);

                exists = blobClient.Exists();
            }

            var logJson = blobClient.DownloadContent().Value.Content.ToString();
            var dumpLog = Newtonsoft.Json.JsonConvert.DeserializeObject<DumpLog>(logJson);

            if (!dumpLog.Success)
            {
                throw new Exception(dumpLog.Message);
            }

            Console.Write(dumpLog.Message);

            blobClient.Delete();
        }
    }
}