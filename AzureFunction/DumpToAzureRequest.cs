namespace Flexerant.MongoDataIO.AzureFunction
{
    public class DumpToAzureRequest
    {
        public string BlobConnectionString { get; set; }
        public string DataBaseName { get; set; }
        public string ClusterUri { get; set; }
    }
}
