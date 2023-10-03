# Semantic Kernel Open AI Plugin example

This repository contains two sample projects.  
One project, `Domain.Platform.Service`, is the service exposing an Open AI plugin. This service is using Semantic Kernel to create a summary for websites.
The other project, `Partner.Copilot.Service`, is the service exposing a rich Copilot experience. Users can ask questions to the REST API endpoint. For summarization of a website, the Open AI plugin of the former project is used.

## Set up

Configure the Azure Open AI settings in the `appsettings.json` file. Both projects can be configured to use the same service.

```jsonc
{
    // ...
    "OpenAI": {
        "ServiceCompletionEndpoint": "https://{yourInstance}.openai.azure.com/",
        "ServiceKey": "{yourKey}",
        "ServiceDeploymentId": "{yourDeployment}",
        "ServiceModelName": "{yourDeployment}",
        "EmbeddingsDeploymentId": "{yourEmbeddingDeployment}"
    }
}
```

When the settings are configured correctly, you can run both projects and try it out yourself.

## Sample usage

### Domain service

To get a summary for a website, you can make a GET request, using the `websiteUrl` in the querystring.

```bash
curl "{{pluginDomainHost}}api/Summarize?websiteUrl=https://jan-v.nl"
```

### Partner service

The partner service also exposes an endpoint to which questions can be asked to.

```bash
curl -X POST -H "Content-Type: application/json" -d '{ "Ask" : "Please create me a summary for the website https://jan-v.nl/" }' "{{pluginPartnerHost}}api/Copilot"
```
