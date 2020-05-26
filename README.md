# Sample - Trigger Blob Download
This sample demonstrates a C# Windows Service [(Topshelf)](http://topshelf-project.com/) that launches a [Powershell](https://www.nuget.org/packages/Microsoft.PowerShell.SDK/) script using [Azcopy](https://github.com/Azure/azure-storage-azcopy) to download a blob. The download process is triggered when a new file is added to blob storage, [Event Grid](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-event-overview) is used to route the event to a service bus queue that is consumed by the C# app.

![Blob Trigger Demo](doc/demo.gif)


## Blob Storage Events
For details on setting up Event Grid please review the following documentation page, [Reacting to Blob storage events](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-event-overview). It provides guidance on:
- Publishing events from blob storage
- Filtering events to ensure events are raised only once the blob is fully committed

# Configuration
The appsettings.json provides the following:
- Powershell script to run, must be located in same folder as dll/executable
- Service bus queue name Event Grid is routing blob storage events to
- Folder to download the blob to
- Service bus connection string
- SaS token for blob storage, this is the query string component (can be generated using [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/))

Here is a sample appsettings.json:
```json
{
  "BlobEventConfig": {
    "PowershellScript": "download_blob.ps1",
    "QueueName": "filearrived",
    "DestinationFolder": "c:\\temp\\blobdestination\\"
  },
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://<sb name>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<sas key>",
    "BlobSaSTokenQueryString": "?sv=2019-02-02&si=files-1724F37AF21&sr=c&sig=nx94H7WcoA1%2BTcLuaAjgKrJicnSTR97jEGPPzkEkDa4%3D"
  }
}

```

## Limitations
This sample is provided as-is to demonstrate concepts, error handling, security, performance and other essential attributes are not explicitly included. 