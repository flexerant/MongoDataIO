using Flexerant.MongoDataIO.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Flexerant.MongoDataIO.AzureFunction
{
    public static class MongoDumpFunction
    {
        [FunctionName("mongodump")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] DumpToAzureRequest req, ILogger log)
        {
            log.LogInformation($"Starting mongodump for {req.DataBaseName}.");

            if (string.IsNullOrWhiteSpace(req.BlobConnectionString)) throw new ArgumentException($"The parameter '{nameof(req.BlobConnectionString)}' cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(req.ClusterUri)) throw new ArgumentException($"The parameter '{nameof(req.ClusterUri)}' cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(req.DataBaseName)) throw new ArgumentException($"The parameter '{nameof(req.DataBaseName)}' cannot be null or whitespace.");

            try
            {
                string mongoBinFolder = Environment.GetEnvironmentVariable("MOGODB_EXE_DIRECTORY", EnvironmentVariableTarget.Process);
                var target = new AzureTarget(mongoBinFolder);
                var result = await Task.Run(() => target.DumpToAzure(req.ClusterUri, req.DataBaseName, req.BlobConnectionString));

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
