using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace exampleApi;

public static class TelemetryConstants
{
    public const string ServiceName = "exampleApi";
}

public static class Signals
{
    public static readonly ActivitySource MyActivitySource = new(TelemetryConstants.ServiceName);
    public static readonly Meter MyMeter = new(TelemetryConstants.ServiceName);
}

public static class SetupOpentelemetry
{
    public static void SetupOpenTelemetry(this WebApplicationBuilder webApplicationBuilder)
    {
        var otelCollectorUrl = Environment.GetEnvironmentVariable("collector") ?? "http://localhost:4317";
        Console.WriteLine("Collector endpoint is: " + otelCollectorUrl);
        var otelCollectorEndpoint = new Uri(otelCollectorUrl);

        webApplicationBuilder.Logging.AddOpenTelemetryLogging(TelemetryConstants.ServiceName, otelCollectorEndpoint);
        webApplicationBuilder.Services.AddOpenTelemetryTracingAndMetrics(TelemetryConstants.ServiceName,otelCollectorEndpoint);
    }

    private static void AddOpenTelemetryLogging(this ILoggingBuilder logging,
        string serviceName, Uri otelCollectorEndpoint)
    {
        var oppResourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);

        logging.AddOpenTelemetry(openTelemetryLoggerOptions =>
        {
            openTelemetryLoggerOptions.SetResourceBuilder(oppResourceBuilder);
            openTelemetryLoggerOptions.AddOtlpExporter(op => { op.Endpoint = otelCollectorEndpoint; });
        });
    }

    private static void AddOpenTelemetryTracingAndMetrics(this IServiceCollection serviceCollection, string serviceName,
        Uri otelCollectorEndpoint)
    {
        IOpenTelemetryBuilder build = serviceCollection.AddOpenTelemetry();
            
            build.ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName))
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
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 5_000;
                    options.Protocol = OtlpExportProtocol.Grpc;
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
                        options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 5_000;
                        options.Protocol = OtlpExportProtocol.Grpc;
                    }
                )
            );
    }

    private static bool IsSwagger(HttpRequest req)
    {
        return req.Path.Value?.Contains("swagger") == true
               || req.Path.Value?.Contains("sw.bundle.js") == true
               || req.Path.Value?.Contains("browserLink") == true;
    }
}