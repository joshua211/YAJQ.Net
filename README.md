# Yet Another Job Queue (YAJQ)
**Preview/Prototype**

## What is YAJQ
* distributed and IoC based ~~task scheduler~~ ~~job scheduler~~ job queue
* designed to be used in applications that use the [.Net Host system](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) and Dependency Injection 
+ simple to use and easy to extend/modify
* Every important system is registered as a serivice and can be easily replaced or changed
* Uses Redis as a fast and persistent database

## Getting started

### Basic

Run ``dotnet add package YAJQ.DependencyInjection`` and <br>
Run ``dotnet add package YAJQ.Hosted``

Add required services
```
builder.Services.AddYAJQ();
builder.Services.AddHostedJobHandler();
```

Enqueue or schedule a job
```
await jobQueue.EnqueueJobAsync(() => SendJobCompleteMessage(id)); //Execute instantly
await jobQueue.ScheduleJobAsync(() => SendJobCompleteMessage(id), DateTimeOffset.Now.AddSeconds(5)); //Execute in 5 seconds

```

### Redis
#### WIP

## Distributed Job Handlers
