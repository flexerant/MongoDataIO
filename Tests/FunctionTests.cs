using Flexerant.MongoDataIO.AzureFunction;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class FunctionTests
    {
        [Theory]
        [InlineData("", "dataBaseName", "clusterUri")]
        [InlineData(null, "dataBaseName", "clusterUri")]
        [InlineData(" ", "dataBaseName", "clusterUri")]
        [InlineData("blobConnectionString", "", "clusterUri")]
        [InlineData("blobConnectionString", null, "clusterUri")]
        [InlineData("blobConnectionString", " ", "clusterUri")]
        [InlineData("blobConnectionString", "dataBaseName", "")]
        [InlineData("blobConnectionString", "dataBaseName", null)]
        [InlineData("blobConnectionString", "dataBaseName", " ")]
        public async Task InvalidParameters(string blobConnectionString, string dataBaseName, string clusterUri)
        {
            DumpToAzureRequest request = new DumpToAzureRequest()
            {
                BlobConnectionString = blobConnectionString,
                DataBaseName = dataBaseName,
                ClusterUri = clusterUri
            };

            var mockLogger = new Mock<ILogger>();

            await Assert.ThrowsAsync<ArgumentException>(() => MongoDumpFunction.Run(request, mockLogger.Object));
        }
    }
}
