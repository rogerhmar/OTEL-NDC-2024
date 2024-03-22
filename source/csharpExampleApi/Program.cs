using exampleApi;
using exampleApi.Service;

// Is needed to export exceptions when older than 1.8.0 
Environment.SetEnvironmentVariable("OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES", "true");

var builder = WebApplication.CreateBuilder(args);

builder.SetupOpenTelemetry();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<SuperService>();
builder.Services.AddSingleton<SuperServiceWithMetrics>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapRoutes();
app.UseStaticFiles();
app.UseDeveloperExceptionPage();
app.Run();

