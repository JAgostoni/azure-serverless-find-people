[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FJAgostoni%2Fazure-serverless-find-people%2Fmaster%2FAzure.Serverless.FindPeopleDemo.Deployment%2Fazuredeploy.json)
[![Build Status](https://jagostoni.visualstudio.com/FindHappyPeople/_apis/build/status/FindHappyPeople-CI?branchName=master)](https://jagostoni.visualstudio.com/FindHappyPeople/_build/latest?definitionId=1?branchName=master)

# Azure Serverless Find People Demo
Complete demo of Azure Functions and Cognitive Services (Bing Search API and Faces API)

# Azure Setup
1. Publish the ARM template in the Deployment project to an existing Azure account
   1. Be sure to change the prefix parameter to something specific to you to avoid any overlap!
2. Publish the Azure Functions project to the newly created azure function app service
3. You can test the function from within the Azure Portal or get the trigger URL for the StartSearch function and test from a browser or Postman, etc.

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

# HTTP Trigger Usage
1. In Postman or even just a web browser, you can just run the HTTP trigger with no params and it will search Bing for people and try and detect "happy" people.
   1. Example: `http://localhost:7071/api/StartSearch`
2. You can override the search critera or the emotion with query string params:
   1. Example `http://localhost:7071/api/StartSearch?search=weird%20people&emotion=surprise`
   2. Supported emotions: angry, happy, contempt, disgust, fear, sad, surprise
3. Matching images will be saved to the Azure storage account created above (or your local storage emulator) in the images container

# CI Build with Azure DevOps
The root of this repository contains a build script, azure-pipelines.yml, which can be used by Azure DevOps to create a CI build pipeline is just a few simple steps:
1. Open a browser to your Azure DevOps account (or create a new one for free)
2. Either select an existing Project or Create a new project
3. In your project, select Pipelines->Builds in the navigation menu/panel and create a NEW pipeline
4. Select this (or your forked) GitHub repo as the source (you may need to link your GitHub account if this is the first connection)
5. Once you have connected the repo, select YAML for build confguration as code
6. When the pipeline editor loads, be sure to select the azure-pipelines.yml file from the root folder
7. Select Save & Queue

Now you have continuous builds setup on your selected repo and branch!

   
