using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DumpToAzureClientConsole
{
    internal class Options
    {
        [Option("username", Required = true)]
        public string Username { get; set; }

        [Option("password", Required = true)]
        public string Password { get; set; }

        [Option("azure-blob-connection-string", Required = true)]
        public string BlobConnectionString { get; set; }

        [Option("webjob-url", Required = true)]
        public string WebjobUrl { get; set; }

        [Option("database-name", Required = true)]
        public string DatabaseName { get; set; }

        [Option("retry-duration-minutes", Required = false, Default = 10)]
        public int RetryDurationMinutes { get; set; } = 10;
    }
}
