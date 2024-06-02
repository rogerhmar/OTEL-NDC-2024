using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace exampleApi;

public static class TelemetryConstants
{
    public const string ServiceName = "exampleApi";
    public const string ServiceVersion = "1.0.0";
    public const int BatchDelay = 5000;
}

public static class SetupOpentelemetry
{
    public static void SetupOpenTelemetry(this WebApplicationBuilder webApplicationBuilder)
    {
        var otelCollectorUrl = Environment.GetEnvironmentVariable("collector") ?? "http://localhost:4317";
        var serviceName = TelemetryConstants.ServiceName;
        var batchDelay = TelemetryConstants.BatchDelay;

        Console.WriteLine("Collector endpoint is: " + otelCollectorUrl);
        var otelCollectorEndpoint = new Uri(otelCollectorUrl);

        var addOpenTelemetryBuilder = webApplicationBuilder.Services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: TelemetryConstants.ServiceVersion,
                    serviceInstanceId: Environment.MachineName
                )
                .AddEnvironmentVariableDetector() // This is needed to detect the environment variables OTEL_RESOURCE_ATTRIBUTES, OTEL_SERVICE_NAME
                .AddAttributes(new Dictionary<string, object> // Additional resource attributes
                    {
                        ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                        ["location"] = Environment.GetEnvironmentVariable("LOCATION") ?? "NDC Oslo",
                    }
                ));
        
        // PS: needed to split - did not work as expected when having one long chain. Gave serviceName = unknown_service:dotnet
        addOpenTelemetryBuilder
            .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.Filter = ctx => !IsSwagger(ctx.Request); // TODO: Task 3
                })
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = otelCollectorEndpoint;
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = batchDelay;
                })
            )
            .WithMetrics(meterProviderBuilder => meterProviderBuilder
                .AddMeter(serviceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = otelCollectorEndpoint;
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = batchDelay;
                })
            );

        webApplicationBuilder.Services.AddLogging(loggingBuilder => loggingBuilder.AddOpenTelemetry(options =>
        {
            options.AddOtlpExporter(op =>
            {
                op.Endpoint = otelCollectorEndpoint;
                op.BatchExportProcessorOptions.ScheduledDelayMilliseconds = batchDelay;
            });
        }));
    }

    private static bool IsSwagger(HttpRequest req)
    {
        return req.Path.Value?.Contains("swagger") == true
               || req.Path.Value?.Contains("sw.bundle.js") == true
               || req.Path.Value?.Contains("browserLink") == true;
    }
}