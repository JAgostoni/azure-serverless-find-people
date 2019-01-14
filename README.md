# Azure Serverless Find People Demo
Complete demo of Azure Functions and Cognitive Services (Bing Search API and Faces API)

# Azure Setup
1. Publish the ARM template in the Deployment project to an existing Azure account
   1. Be sure to change the prefix parameter to something specific to you to avoid any overlap!
2. Publish the Azure Functions project to the newly created azure function app service

# Local Setup and Testing
1. Ensure you have the (Azure Function)[https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local] tools installed locally as well as the (Azure Storage Emulator)[https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator]
2. Create a local.settings.json in the functions project, similar to this.  Replace the namespace, keys and endpoint with values from your Azure account as created above.

````javascript
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ProcessQueue": "<your sb connection string>",
    "BingSearchKey": "<your Bing key>",
    "CVKey": "<Your Cognitive Services Key",
    "CVEndpoint": "<your Cognitive Services Endpoint>"
  }
}
````

3. In Visual Studio, just press F5 and you'll see the HTTP trigger endpoint displayed in the functions console window
