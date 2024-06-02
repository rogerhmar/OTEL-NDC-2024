using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace exampleApi.Service
{
    public class SuperServiceWithMetrics
    {
        private readonly ILogger<SuperServiceWithMetrics> _logger;

        public SuperServiceWithMetrics(ILogger<SuperServiceWithMetrics> logger, Meter meter)
        {
            _logger = logger;
            _counter = meter.CreateCounter<long>("SuperServiceCounter", null, "This counts a lot");
        }
        
        // TODO: Task 22
        private readonly Counter<long> _counter;

        public void Increment(int inc = 1)
        {
            _logger.LogInformation("Incrementing counter by {inc}", inc);
            var tagList = new TagList {{"MetricType", "errorCounter"}};
            _counter.Add(inc, tagList);
        }
    }
}
