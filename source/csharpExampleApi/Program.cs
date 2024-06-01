using exampleApi;
using exampleApi.Service;

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
app.UseStaticFiles();
app.UseDeveloperExceptionPage();

app.MapRoutes();

app.Run();

