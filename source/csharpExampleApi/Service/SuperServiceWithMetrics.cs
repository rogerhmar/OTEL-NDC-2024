using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace exampleApi.Service
{
    public class SuperServiceWithMetrics
    {
        private readonly ILogger<SuperServiceWithMetrics> _logger;

        public SuperServiceWithMetrics(ILogger<SuperServiceWithMetrics> logger)
        {
            _logger = logger;
        }
        
        // TODO: Task 22
        private readonly Counter<long> _counter = 
            Signals.MyMeter.CreateCounter<long>("SuperServiceCounter",null,"This counts a lot");

        public void Increment(int inc = 1)
        {
            _logger.LogInformation("Incrementing counter by {inc}", inc);
            var tagList = new TagList {{"MetricType", "errorCounter"}};
            _counter.Add(inc, tagList);
        }
    }
}
