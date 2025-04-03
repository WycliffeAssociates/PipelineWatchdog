# Description

This tool listens to the main rendering pipeline bus and records all of the repos it has seen. It will then on a schedule check all of the repos in WACS against what it has seen and then sends 
repos to the pipeline again to make sure the creation is handled or the delte is handled.

# Building
You can use the .net cli to build this with `dotnet build` alternatively you could use docker to build this with `docker build . --file PipelineWatchdog/Dockerfile`

# Configuration
The system has several different configuration options that can be configured via standard IConfiguration means (arguments, environment variables, configuration files, etc.)

- IntervalInMinutes: The delay between runs of the watchdog in minutes
- WACSUrl: The base url of the wacs server to use as our base

There are also a couple connection strings

- ServiceBus: A connection string for the bus to listen to and to send events to
- TableStorage: A connection to an azure table storage account that will hold our list of repos

# Running

Either a `dotnet run` or a `docker run` will get this going