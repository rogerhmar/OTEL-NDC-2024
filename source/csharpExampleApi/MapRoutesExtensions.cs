using System.Diagnostics;
using exampleApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace exampleApi;

public static class MapRoutesExtensions
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapGet("/", async ctx =>
        {
            ctx.Response.Headers.ContentType =
                new Microsoft.Extensions.Primitives.StringValues("text/html; charset=UTF-8");
            await ctx.Response.SendFileAsync("wwwroot/index.html");
        }).WithName("root");

        // TODO: Task 4
        app.MapGet("/serial",async (SuperService superService) => await superService.Dependency1("AnotherMethod"))
            .WithName("serial");

        app.MapGet("/parallel", async (SuperService superService, ILogger<SuperService> logger) =>
        {
            logger.LogInformation("Starting parallel");

            var tasks = new[]
            {
                superService.Dependency1("First"),
                superService.Dependency2("Second"),
                superService.Dependency1("Third"),
                superService.Dependency2("Fourth")
            };
            var timer = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            return new Message($"Completed all dependencies in {timer.ElapsedMilliseconds} ms");
        }).WithName("parallel");

        app.MapGet("/hi",
            async (SuperService superService, IHttpClientFactory httpClientFactory, ILogger<SuperService> logger, CancellationToken cancellationToken) =>
            {
                var client = httpClientFactory.CreateClient();
                logger.LogInformation("Starting parallel and hi to Java");
                
                var timer = Stopwatch.StartNew();

                var timedCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timedCts.Token, cancellationToken);
                
                var fromJava = await client.GetStringAsync("http://localhost:5010/hello", cts.Token);
                await superService.Dependency1("First");
                await superService.Dependency2("Second");

                return new Message($"Completed all dependencies in {timer.ElapsedMilliseconds} ms. Kotlin says: {fromJava}");
            }).WithName("kotlin-dependency");

        app.MapGet("/error",
            (HttpResponse response) =>
            {
                Activity.Current?.AddEvent(new ActivityEvent("This is an event added to the span"));
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                response.StatusCode = 400;
            }).WithName("error");

        app.MapGet("/throwEx",
                (HttpResponse response) => { throw new Exception("This is an exception thrown from the controller"); })
            .WithName("throwEx");

        app.MapGet("/remove", () => { throw new Exception("This should not be found in Tempo!"); }).WithName("remove");

        app.MapPost("/metric/inc/{num}",
            (HttpResponse response, [FromRoute(Name = "num")] int increment,
                SuperServiceWithMetrics superServiceWithMetrics) =>
            {
                superServiceWithMetrics.Increment(increment);
                response.StatusCode = 202;
            }).WithName("metric-inc");

        app.MapGet("/hello", () => "Hello Kotlin!").WithName("hello");
    }
}
