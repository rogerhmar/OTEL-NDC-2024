# OpenTelemetry Workshop
This setup is based on: https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/examples/demo

## Alternative setup
If you want a simplified setup with a single container, you can use https://github.com/grafana/docker-otel-lgtm/. This is mained by Grafana.

## Prerequisites
* Tool for building and running containers, e.g. Docker Desktop, Podman or Rancher Desktop - Including Compose.
* Dotnet 8

## Overall Architecture
```mermaid
flowchart LR
    A["App1(example)"] --> B("OpenTelemetry Collector \n :4317 (gRPC)")
    H["dependency1"] --> B
    I["dependency2"] --> B
    J["dependency3"] --> B
    B --> D("Loki \n :3100") --> G("Grafana")
    B --> E("Tempo \n :3200") --> G
    B --> F(Prometheus \n :9090)--> G

    style A fill:red
    style B fill:green
    style H fill:blue
    style I fill:blue
    style J fill:blue
    style D fill:yellow
    style E fill:orange
    style F fill:cyan
    style G fill:purple
```

- Your application send data directly to the OTEl collector over gRPC
- Prometheus scrapes data from the OTEL collector
- Collector writes data to Loki and Tempo
- Grafana uses the 3 sources to display data

## Run Infrastructure

### Verify prerequisites
* Docker is running? Run `docker ps`. You see `CONTAINER ID` and a lot of other stuff.
  * `error during connect` means Rancher Desktop/Podman Desktop/Docker Desktop has not been starterd
* Docker-Compose is installed? Run `Docker-Compose -v`
* Dotnet is installed? `dotnet --version`. This should display the version.

### Run the infrastructure
Start by running `docker compose up` add `-d` to run detatched (just start it without displaying all logs)
To clean up any docker container run `docker-compose down` from this folder.

### Run the demo application - ExampleApi
Run it with `dotnet run` or inside an IDE. This will give you more about URLs you can visit.

## Instrumentation
There are 2 options for setting up OpenTelemetry in .NET applications.
* Setup with code (With the option of using both manual and automatic instrumentation)
* No-code automatic setup of automatic instrumentation

### Manual setup
The example app uses this setup. Refer to [SetupOpentelemetry](source/example/SetupOpentelemetry.cs) to see how this may be done. For more infomation and examples refer to
* https://opentelemetry.io/docs/languages/net/

### (fully) Automatic instrumentation
This setup has include 3 containers (dependency1..3) with automatic instrumentation. The 2 main solutions for doing this is:
* Download and run `otel-dotnet-auto-install.sh` or `OpenTelemetry.DotNet.Auto.psm1` or
* Include Nuget `OpenTelemetry.AutoInstrumentation`

We have used the latter approch. Refer to dependency1 [docker-compose.yaml](./docker-compose.yaml) for an example. This example includes ENV variables for easier debugging.

For more information refer to:
* https://opentelemetry.io/docs/zero-code/net/


## Example App
```
    .
    ├── exampleAPI.http        <- To run HTTP command. An alternative to using Swagger
    ├── MapRoutesExtensions.cs <- Sets up the routes
    ├── SetupOpentelemetry.cs  <- All OpenTelemetry setup for Logging, Tracing and Metrics
    └── Program.cs...          <- All the normal stuff
```

Startup app and go to `http://localhost:5000/`


# Doing Stuff

## .NET - Tracing
To look at the traces go to ´Grafana´ and open ´Explore´, and choose ´Tempo´ as data source. Make sure tab ´Search´ is chosen, and not ´TraceQL´

### Task 1: Get it up and running
* Start the infrastructure as in this [section](#run-infrastructure)
* Start the `ExampleApi`
* Open the webpage in `ExampleApi` and become familiar with the links

### Task 2: Is it connected?
* Understand the setup
  * Refer to all comments starting with "SETUP"
* Verify that `ExampleApi` is sending Metrics and Traces to the OpenTelemetry
  * How did you verify it?
* Try changing the ENV-variable `collector`
  * What happend?

### Task 3: Reducing noise
* Try disabling the RequestFilter
* (use swagger) to send requests to the APIs
  * `/remove`
* Update the request filter (input to AddElementsOpenTelemetry) to remove this endpoint
* Verify in Tempo that you succeeded.


### Task 4: Use the API and understand the delay
* (use swagger) to send requests to the APIs
  * `/parallel`
  * `/serial`
* (Use Jaeger) to understand what is causing the delay?
* Find out why tracing is missing in `ThisNeedsToBeTraced` 

### Task 5: Use the API and get exceptions
* (use swagger) to send requests to the APIs
  * `/error`
* Look at trace in Jaeger and add imprevements

## .NET Metrics
### Task 21: Understanding the data flow
* I have added a custom metric to `SuperServiceWithMetrics`
* Use the endpoint `/metric/inc` to increment this. Refer to swagger.
* Verify that this is working
  * Find the metric in the output of the Open Telemetry Collector http://localhost:8889/metrics
  * Find the metric in prometheus: http://localhost:9090/graph
  * Find the metric in Grafana: http://localhost:3000/explore

### Task 22: Add a custom metrics
* Add another counter to the setup
* Verify that this has been added to your metrics

### Agnostic Bonus 1: Run k6 test and observe the system under load
* Install k6. E.g. by running this https://dl.k6.io/msi/k6-latest-amd64.msi
* Docs: https://k6.io/docs/
* Update URL in `script.js` located inside the `test`-folder - `k6 run script.js`
* See if you can make the application struggle a bit by e.g. run the script for a longer period of time.
