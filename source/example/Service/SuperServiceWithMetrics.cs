using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace exampleApi.Service
{
    public class SuperServiceWithMetrics(ILogger<SuperServiceWithMetrics> logger, Meter meter)
    {
        private readonly Counter<long> _counter = meter.CreateCounter<long>("super_service_counter", null, "This counts a lot");

        public void Increment(int inc = 1)
        {
            logger.LogInformation("Incrementing counter by {inc}", inc);
            var tagList = new TagList {{"MetricType", "errorCounter"}};
            _counter.Add(-1, tagList);
        }
    }
}
