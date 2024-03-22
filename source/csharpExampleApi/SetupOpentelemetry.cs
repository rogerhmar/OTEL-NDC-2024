using System.Diagnostics;
using System.Diagnostics.Metrics;
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
    public static readonly Meter MyMeter = new Meter(TelemetryConstants.ServiceName);
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
        serviceCollection.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder.AddService(
                    serviceName: serviceName,
                    serviceVersion: "1.0.0"
                )
            )
            .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                .AddSource(serviceName)
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSqlClientInstrumentation(sqlClientInstrumentationOptions =>
                    sqlClientInstrumentationOptions.SetDbStatementForText = true)
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.Filter = ctx => !IsSwagger(ctx.Request); // TODO: Task 3
                })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = otelCollectorEndpoint;
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 5_000;
                })

            )
            .WithMetrics(meterProviderBuilder => meterProviderBuilder
                .AddMeter(serviceName)
                .AddHttpClientInstrumentation()
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otelCollectorEndpoint;
                        options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 5_000;
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