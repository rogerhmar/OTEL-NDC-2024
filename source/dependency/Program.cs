using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using dependencyApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TestingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/slow", async () =>
    {
        await Task.Delay(600);
        return new { message = "This is a slow response" };
    })
    .WithName("SlowRequest")
    .WithOpenApi();

app.MapGet("/health", () => Task.FromResult(new { status = "OK" }))
    .WithName("Health")
    .WithOpenApi();

app.MapPost("/chain", async ([FromBody]string[] hostWithPort, [FromServices] ILoggerFactory factory) =>
    {
        var logger = factory.CreateLogger("ChainRequest");

        var l = "[" + string.Join(',', hostWithPort) + "]";
        logger.LogInformation("Received request with host and port: {0}", l);
        logger.LogInformation("len: {0}", hostWithPort.Length);

        var currentHost = hostWithPort.FirstOrDefault();
        if (currentHost is null)
        {
            return Results.Ok();
        }
        
        var slowUri = new Uri($"http://{currentHost}/slow");
        var chainUri = new Uri($"http://{currentHost}/chain");
        var listOfRemainingHosts = hostWithPort.Skip(1).ToArray();
        switch (hostWithPort.Length)
        {
            case 1:
            {
                var res = await new HttpClient().GetAsync(slowUri);
                return Results.Ok(res.Content.ReadAsStringAsync());
            }
            default:
            {
                var hostsAsJsonString = System.Text.Json.JsonSerializer.Serialize(listOfRemainingHosts);
                logger.LogInformation("List: " + hostsAsJsonString);
                var res = await new HttpClient().PostAsync(chainUri,new StringContent(hostsAsJsonString,Encoding.UTF8, "application/json"));
                return Results.Ok(res.Content.ReadAsStringAsync());
            }
        } 
    })
    .WithName("ChainRequest")
    .WithOpenApi();

app.MapGet("/test", async (HttpResponse response, [FromServices] TestingService testingService) =>
    {
        var failingServices = await testingService.Test();
        if (failingServices.Count > 0)
        {
            response.StatusCode = 500;
        }
        return new { FailingServices = failingServices, OK = response.StatusCode == 200};
    })
    .WithName("test");

app.Run();

record DependencyApi();