using Flexerant.MongoDataIO.AzureFunction;
using Flexerant.MongoDataIO.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class FunctionTests : IClassFixture<BlobStorageFixture>
    {
        private readonly BlobStorageFixture _blobStorageFixture;

        public FunctionTests(BlobStorageFixture blobStorageFixture)
        {
            _blobStorageFixture = blobStorageFixture;
        }

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

        [Fact]
        public async Task RunFunction()
        {
            using (var tempenv = new TempEnvironment(Helpers.GetMongoToolesFolder()))
            {
                using (var db = new MockMongoDatabase())
                {
                    DumpToAzureRequest request = new DumpToAzureRequest()
                    {
                        BlobConnectionString = _blobStorageFixture.ConnectionString,
                        DataBaseName = db.DatabaseName,
                        ClusterUri = db.ConnectionString
                    };

                    var mockLogger = new Mock<ILogger>();
                    var response = await MongoDumpFunction.Run(request, mockLogger.Object);
                    var ok = response as Microsoft.AspNetCore.Mvc.OkObjectResult;
                    var result = ok.Value as DumpToAzureResult;

                    Assert.False(string.IsNullOrWhiteSpace(result.ContainerName));
                    Assert.False(string.IsNullOrWhiteSpace(result.BlobName));
                }
            }
        }
    }
}
