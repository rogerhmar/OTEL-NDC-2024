using System.Diagnostics;
using System.Diagnostics.Metrics;
using exampleApi;
using exampleApi.Service;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.SetupOpenTelemetry();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<SuperService>();
builder.Services.AddSingleton<SuperServiceWithMetrics>();
builder.Services.AddSingleton<TestingService>();

// The Standard .NET way - Use System.Diagnostics for traces 
builder.Services.AddSingleton(new ActivitySource(TelemetryConstants.ServiceName)); // Dotnet 

// The more OpenTelemetry compliant way - API shim on top of System.Diagnostics https://opentelemetry.io/docs/languages/net/shim/
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(TelemetryConstants.ServiceName)); 

builder.Services.AddSingleton(new Meter(TelemetryConstants.ServiceName));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseDeveloperExceptionPage();
app.MapRoutes();

app.Run();

