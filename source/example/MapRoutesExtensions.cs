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

        // TODO: Task 4
        app.MapGet("/serial", async ([FromServices]SuperService superService) => await superService.InternalDependency1("AnotherMethod"))
            .WithName("serial");

        app.MapGet("/parallel", async ([FromServices]SuperService superService, [FromServices]ILoggerFactory factory) =>
        {
            factory.CreateLogger("parallel").LogInformation("Starting parallel");

            var tasks = new[]
            {
                superService.InternalDependency1("First"),
                superService.InternalDependency2("Second"),
                superService.InternalDependency1("Third"),
                superService.InternalDependency2("Fourth")
            };
            var timer = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            return new Message($"Completed all dependencies in {timer.ElapsedMilliseconds} ms");
        }).WithName("parallel");

        app.MapGet("/error", (HttpResponse response) =>
            {
                Activity.Current?.AddEvent(new ActivityEvent("This is an event added to the span"));
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                response.StatusCode = 400;
            })
            .WithName("error");

        app.MapGet("/throwEx",
                (HttpResponse _) => { throw new Exception("This is an exception thrown from the controller"); })
            .WithName("throwEx");

        app.MapGet("/remove", () => { throw new Exception("This should not be found in Tempo!"); })
            .WithName("remove");

        app.MapPost("/metric/inc/{num}",
            (HttpResponse response, [FromRoute(Name = "num")] int increment, [FromServices]SuperServiceWithMetrics superServiceWithMetrics) =>
            {
                superServiceWithMetrics.Increment(increment);
                response.StatusCode = 202;
                return Results.Content("Incrementing counter by " + increment +
                                       " Have a look in the Counter Dashboard! http://localhost:3000/dashboards");
            }).WithName("metric-inc");
    }
}
