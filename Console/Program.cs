using CommandLine;
using Flexerant.MongoDataIO.Core;
using System.IO;

namespace Flexerant.MongoDataIO.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<DumpFileOptions, DumpToAzureOptions, RestoreFileOptions, RestoreFromAzureOptions>(args)
                .MapResult(
                   (DumpFileOptions opts) => DumpToFile(opts),
                   (DumpToAzureOptions opts) => DumpToAzure(opts),
                   (RestoreFileOptions opts) => RestoreFromFile(opts),
                   (RestoreFromAzureOptions opts) => RestoreFromAzure(opts),
                   errs => 1);
        }

        static int DumpToFile(DumpFileOptions opts)
        {
            var target = new FileTarget(GetMongoBinFolder().FullName);

            target.DumpToFile(opts.ClusterUri, opts.DataBaseName, opts.OutputFolder, data => System.Console.Write(data));

            return 0;
        }

        static int DumpToAzure(DumpToAzureOptions opts)
        {
            var target = new AzureTarget(GetMongoBinFolder().FullName);

            target.DumpToAzure(opts.ClusterUri, opts.DataBaseName, opts.BlobConnectionString, data => System.Console.Write(data));

            return 0;
        }

        static int RestoreFromFile(RestoreFileOptions opts)
        {
            var target = new FileTarget(GetMongoBinFolder().FullName);

            target.RestoreFromFile(opts.ClusterUri, opts.DataBaseName, opts.ArchivePath, data => System.Console.Write(data));

            return 0;
        }

        static int RestoreFromAzure(RestoreFromAzureOptions opts)
        {
            var target = new AzureTarget(GetMongoBinFolder().FullName);

            target.RestoreFromAzure(opts.ClusterUri, opts.BlobConnectionString, opts.BlobContainer, opts.BlobName, data => System.Console.Write(data));

            return 0;
        }

        static DirectoryInfo GetMongoBinFolder()
        {
            return new DirectoryInfo(Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, @"mongodb_db_tools", "bin"));
        }
    }
}
