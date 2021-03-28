using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    [Verb("dump-to-azure")]
    class DumpToAzureOptions : DumpOptions
    {
        [Option("azure-blob-connection-string", Required = true)]
        public string BlobConnectionString { get; set; }
    }
}
