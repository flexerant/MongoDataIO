using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    [Verb("restore-file")]
    class RestoreFileOptions : RestoreOptions
    {
        [Option("source-db-name", Required = true)]
        public string SourceDataBaseName { get; set; }

        [Option("destination-db-name", Required = true)]
        public string DestinationDataBaseName { get; set; }

        [Option("archive", Required = true)]
        public string ArchivePath { get; set; }
    }
}
