using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    [Verb("restore-file")]
    class RestoreFileOptions : RestoreOptions
    {
        [Option("mongodb-db-name", Required = true)]
        public string DataBaseName { get; set; }

        [Option("archive", Required = true)]
        public string ArchivePath { get; set; }
    }
}
