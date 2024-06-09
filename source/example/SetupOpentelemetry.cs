using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace exampleApi;

public static class TelemetryConstants
{
    public const string ServiceName = "exampleApi";
}

public static class SetupOpentelemetry
{
    public static void SetupOpenTelemetry(this WebApplicationBuilder webApplicationBuilder)
    {
        var addOpenTelemetryBuilder = webApplicationBuilder.Services.AddOpenTelemetry().ConfigureResource(builder =>
        {
            // builder.AddService(
            //         serviceName: TelemetryConstants.ServiceName,
            //         serviceVersion: Assembly.GetEntryAssembly()
            //             ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            //         serviceInstanceId: Environment.MachineName,
            //         autoGenerateServiceInstanceId: false
            //     )
            //     //.AddEnvironmentVariableDetector()
            //     .AddAttributes(new Dictionary<string, object> // Additional resource attributes
            //         {
            //             ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            //             ["location"] = Environment.GetEnvironmentVariable("LOCATION") ?? "NDC Oslo",
            //         }
            //     );
        });
            
        
        // PS: needed to split - did not work as expected when having one long chain. Gave serviceName = unknown_service:dotnet
        addOpenTelemetryBuilder
            .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                    .AddSource(TelemetryConstants.ServiceName) // Include this application as a source
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.RecordException = true;
                        o.Filter = ctx => !IsSwagger(ctx.Request); // TODO: Task T1
                    })
                    .AddHttpClientInstrumentation(o => o.RecordException = true)
                // .AddOtlpExporter() //  Signal-specific AddOtlpExporter methods and the cross-cutting UseOtlpExporter method being invoked on the same IServiceCollection is not supported.
            )
            .WithMetrics(meterProviderBuilder => meterProviderBuilder
                .AddMeter(TelemetryConstants.ServiceName) // Include this application as a meter
                .AddAspNetCoreInstrumentation() // TODO: Task M2 - Look into this
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()); // This is in beta - Need to create a complete dashboard - Ref https://grafana.com/grafana/dashboards/19896-asp-net-otel-metrics-from-otel-collector/
            
                //.AddOtlpExporter()); //  Signal-specific AddOtlpExporter methods and the cross-cutting UseOtlpExporter method being invoked on the same IServiceCollection is not supported.
 
        webApplicationBuilder.Services.AddLogging(loggingBuilder => loggingBuilder.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true; // This is needed to include the formatted message in the log. E.g. Hosting environment: {EnvName} -> Hosting environment: Development
            options.IncludeScopes = true; // This is needed to include the scope information in the log
            // options.AddOtlpExporter(); //  Signal-specific AddOtlpExporter methods and the cross-cutting UseOtlpExporter method being invoked on the same IServiceCollection is not supported.
            options.AddProcessor(new MyProcessor());
        }));
        
        addOpenTelemetryBuilder.UseOtlpExporter(); // Set OTLP exporter for all signals. Endpoint is set in OTEL_EXPORTER_OTLP_ENDPOINT environment variable
    }

    private static bool IsSwagger(HttpRequest req)
    {
        return req.Path.Value?.Contains("swagger") == true
               || req.Path.Value?.Contains("sw.bundle.js") == true
               || req.Path.Value?.Contains("browserLink") == true;
    }
    
  
    class MyProcessor : BaseProcessor<LogRecord>
    {
        public override void OnEnd(LogRecord record)
        {
            // Set debug here, and watch the record before it is sent to the exporter
            var a = 1;
        }

        public override void OnStart(LogRecord data)
        {
            base.OnStart(data);
        }
    }

}