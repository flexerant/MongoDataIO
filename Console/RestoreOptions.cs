using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    class RestoreOptions
    {
        [Option("mongodb-uri", Required = true)]
        public string ClusterUri { get; set; }
    }
}
