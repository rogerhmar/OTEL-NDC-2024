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
        }).WithName("index_html");
        
        app.MapGet("/serial", async ([FromServices]SuperService superService) => await superService.InternalDependency1("AnotherMethod"))
            .WithName("serial");

        app.MapGet("/parallel", async ([FromServices]SuperService superService, [FromServices]ILoggerFactory factory) =>
        {
            var logger = factory.CreateLogger("parallel");
            logger.LogInformation("Starting parallel");

            var tasks = new[]
            {
                superService.InternalDependency1("First"),
                superService.InternalDependency2("Second"),
                superService.InternalDependency1("Third"),
                superService.InternalDependency2("Fourth")
            };
            var timer = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            timer.Stop();
            
            logger.LogInformation("Parallel completed in {ElapsedMilliseconds} ms", timer.ElapsedMilliseconds);
            
            return new Message($"Completed all dependencies in {timer.ElapsedMilliseconds} ms");
        }).WithName("parallel");

        app.MapGet("/error", (HttpResponse response) =>
            {
                Activity.Current?.AddEvent(new ActivityEvent("This is an event added to the span"));
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                // TODO: Add a tag, and find it in Tempo
                response.StatusCode = 400;
            })
            .WithName("error");

        // TODO: Task T3
        app.MapGet("/throwEx",
                (HttpResponse _) => { throw new Exception("This is an exception thrown from the controller"); })
            .WithName("throwEx");

        app.MapGet("/remove", () => new {message="This should not be found in Tempo!"})
            .WithName("remove");
        
        // TODO Task M1
        app.MapGet("/metric/inc/{num}",
            (HttpResponse response, [FromRoute(Name = "num")] int increment, [FromServices]SuperServiceWithMetrics superServiceWithMetrics) =>
            {
                superServiceWithMetrics.Increment(increment);
                response.StatusCode = 202;
                return Results.Content($"Incrementing counter by {increment}. Have a look in the Counter Dashboard! http://localhost:3000/dashboards");
            }).WithName("metric-inc");
    }
}
