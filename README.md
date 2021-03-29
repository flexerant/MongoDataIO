# MongoDataIO

**MongoDataIO** is a wrapper for [`mongodump.exe`](https://docs.mongodb.com/database-tools/mongodump/#mongodb-binary-bin.mongodump) and [`mongorestore.exe`](https://docs.mongodb.com/database-tools/mongorestore/#mongodb-binary-bin.mongorestore) that supports writing and reading to/from streams. The assembly `Flexerant.MongoDataIO.Core` abstracts the command line utilities so that they can easily be used in any .NET Core application.

## Motivation/Inspiration

This project was inspired by a need to automatically back-up [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) databases during the [Octopus Cloud](https://octopus.com/pricing/cloud) deployment process.

### The problem

**MongoDB Atlas**'s security model includes an IP address whitelist, allowing you to restrict database access to only trusted sources. [**Octopus Cloud**'s worker pools pull from a large range of IP addresses](https://help.octopus.com/t/worker-pool-ip-address-range/24242/4) making it impracticle to whitelist. Also, the backed up databases need to be saved to [Azure Blob Storage](https://azure.microsoft.com/en-ca/services/storage/blobs/) for retrieval in case a deployment needs to be rolled back.

### The solution

To overcome the aforementioned problem, this project includes an [Azure Function](https://azure.microsoft.com/en-ca/services/functions/) that invokes `Flexerant.MongoDataIO.Core` and streams the output of `mongodump.exe` to **Azure Blob Storage**.

## Azure Function

The [AzureFunction project](https://github.com/flexerant/MongoDataIO/tree/main/AzureFunction) can be deployed to **Azure Functions** directly through Visual Studio, using the publish feature. This deploys the code needed to back up the **MongoDB Atlas** database and save it to **Azure Blob Storage**.

**Note:** Be sure to set the `MOGODB_EXE_DIRECTORY` environment variable for the **Azure Function**. If deploying the function as is, the value for the `MOGODB_EXE_DIRECTORY` environment variable is `C:\home\site\wwwroot\mongodb_db_tools\bin`.

## Octopus Powershell Script

The Powershell script is an example of how to invoke the **Azure Function** from within your **Octopus Cloud** deployment step.

```powershell

$functionUri = $OctopusParameters["MongoDump.FunctionUrl"]
$code= $OctopusParameters["MongoDump.FunctionAccessKey"]
$uri = $functionUri + '?code=' + $code
$blobConnectionString = $OctopusParameters["MongoDump.AzureBlobConnectionString"]
$dataBaseName = $OctopusParameters["MongoDump.DatabaseName"]
$clusterUri = $OctopusParameters["MongoDump.MongoConnectionString"]

$params = @{"blobConnectionString"=$blobConnectionString;
 "dataBaseName"=$dataBaseName;
 "clusterUri"=$clusterUri;
}

$json = ($params|ConvertTo-Json)

$response = Invoke-RestMethod -Uri $uri -Method Post -Body $json -ContentType "application/json"

```

`MongoDump.FunctionUrl` is the url for the **Azure Function**, `MongoDump.FunctionAccessKey` is the access key for the **Azure Function**, `MongoDump.AzureBlobConnectionString` is the **Azure Blob Storage** connection string, `MongoDump.DatabaseName` is the **MongoDB Atlas** database to be backed up, and `MongoDump.MongoConnectionString` is the connection string for the **MongoDB Atlas** database.

# Acknowlegements

- [MongoDB Database Tools](https://github.com/mongodb/mongo-tools)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)

# Want to contribute?

Fork the project, make your changes, and send us a PR. You can compile the project with Visual Studio 2019 and the .NET Core 3.1 framework.
