using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    class DumpOptions
    {
        [Option("mongodb-db-name", Required = true)]
        public string DataBaseName { get; set; }

        [Option("mongodb-uri", Required = true)]
        public string ClusterUri { get; set; }
    }
}
