# Serilog.Sinks.AzCosmosDB
[![.NET](https://github.com/tghamm/serilog-sinks-azcosmosdb/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tghamm/serilog-sinks-azcosmosdb/actions/workflows/dotnet.yml) [![Nuget](https://img.shields.io/nuget/dt/Serilog.Sinks.AzCosmosDB)](https://www.nuget.org/packages/Serilog.Sinks.AzCosmosDB/)

A relatively new Serilog sink that writes to Azure CosmosDB and supports PartitionKey and PeriodicBatching for better performance. This code maintained and is based on [serilog-sinks-azurecosmosdb](https://github.com/mahdighorbanpour/serilog-sinks-azurecosmosdb) and adapted to use modern patterns and practices.

## Getting started
You can start by installing the [NuGet package](https://www.nuget.org/packages/Serilog.Sinks.AzCosmosDB).



Configure the logger by calling `WriteTo.AzCosmosDB(<client>, <options>)`

```C#
var builder =
    new CosmosClientBuilder(configuration["AppSettings:AzureCosmosUri"], configuration["AppSettings:AzureCosmosKey"])
        .WithConnectionModeGateway();
var client = builder.Build();

var logger = new LoggerConfiguration()
    .WriteTo.AzCosmosDB(client, new AzCosmosDbSinkOptions()
    {
        DatabaseName = "TestDb"
    })
    .CreateLogger();
```
## PartitionKey

The default partition key name is <b>/UtcDate</b> although it can be overrided using a parameter like below:

```C#
Log.Logger = new LoggerConfiguration()
                .WriteTo.AzCosmosDB(client, new AzCosmosDbSinkOptions()
                    {
                        DatabaseName = "TestDb",
                        PartitionKey = "MyCustomKeyName"
                    })
                .CreateLogger();
```

## IPartitionKeyProvider
The DefaultPartitionkeyProvider will generate a utc date string with the format "dd.MM.yyyy". If you want to override it, you need to define a class and implement IPartitionKeyProvider interface and pass an instance of it in the arguments list.  There is an example of this in the 
TestRunner project in the repository.

## TTL (Time-to-live)

Azure CosmosDB makes it easier to prune old data with the support of Time To Live (TTL) and so does this Sink. `AzCosmosDB` Sink offers TTL at two levels.

### Enable TTL at collection level.

The sink supports TTL at the collection level, if the collection does not already exist.
 
To enable TTL at the collection level, set the **TimeToLive** parameter in code.

```C#
.WriteTo.AzCosmosDB(client, new new AzCosmosDbSinkOptions() { TimeToLive = TimeSpan.FromDays(7)})
```
If the collection in CosmosDB doesn't exist, it will create one and set TTL at the collection level causing all log messages older than 7 days to purge.


### Enable TTL at inidividual log message level.

The sink supports TTL at the individual message level as well. This allows developers to retain log messages of high importance longer than those of lesser importance.

```C#
logger.Information("This message will expire and purge automatically after {@_ttl} seconds", 60);

logger.Information("Log message will be retained for 30 days {@_ttl}", 2592000); // 30*24*60*60

logger.Information("Messages of high importance will never expire {@_ttl}", -1); 
```

See [TTL behavior](https://docs.microsoft.com/en-us/azure/cosmos-db/time-to-live) in CosmosDB documentation for an in depth explianation.

>Note: `{@_ttl}` is a reserved expression for TTL.

## Performance
THe sink buffers log internally and flushes to Azure CosmosDB in batches using `Serilog.Sinks.PeriodicBatching` and is configurable. However, performance and throughput highly depends on type of Azure CosmosDB subscription you have.

## System.Text.Json Support in .NetStandard 2.1, .Net 6.0 and .Net 7.0
For additional performance, this sink has implemented the System.Text.Json serializer under the hood for performance in more modern versions of .Net.  THe cosmos library intends to migrate away from `Newtonsoft.Json` in a future major release, so this will eventually remove a dependency when that happens.
