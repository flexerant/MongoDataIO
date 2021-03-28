using CommandLine;

namespace Flexerant.MongoDataIO.Console
{
    [Verb("dump-file")]
    class DumpFileOptions : DumpOptions
    {
        [Option("out", Required = true)]
        public string OutputFolder { get; set; }
    }
}
