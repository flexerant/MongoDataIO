using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace Tests
{
    public class BlobStorageFixture : IDisposable
    {
        private readonly Process _process;
        private static readonly IConfiguration _configuration;

        public string ConnectionString { get; private set; }

        static BlobStorageFixture()
        {
            _configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();
        }

        public BlobStorageFixture()
        {
            this.ConnectionString = _configuration["AzureStorage:ConnectionString"];

            _process = new Process
            {
                StartInfo = { UseShellExecute = false, FileName = _configuration["AzureStorage:EmulatorPath"] }
            };

            StartAndWaitForExit("stop");
            StartAndWaitForExit("clear all");
            StartAndWaitForExit("start");
        }

        public void Dispose()
        {
            StartAndWaitForExit("stop");
        }

        void StartAndWaitForExit(string arguments)
        {
            _process.StartInfo.Arguments = arguments;
            _process.Start();
            _process.WaitForExit(10000);
        }
    }
}
