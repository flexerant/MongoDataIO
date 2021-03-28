﻿using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    [Verb("restore-from-azure")]
    class RestoreFromAzureOptions : RestoreOptions
    {
        [Option("azure-blob-connection-string", Required = true)]
        public string BlobConnectionString { get; set; }

        [Option("azure-blob-container", Required = true)]
        public string BlobContainer { get; set; }

        [Option("azure-blob-name", Required = true)]
        public string BlobName { get; set; }
    }
}
