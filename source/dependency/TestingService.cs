namespace dependencyApi;

public class TestingService(ILogger<TestingService> logger)
{
    public async Task<List<string>> Test()
    {
        var failingTests = new List<string>();
        
        var endpoints = new Dictionary<string, string>
        {
            ["grafana"] = "http://grafana:3000/metrics",
            ["loki"] = "http://loki:3100/metrics",
            ["tempo"] = "http://tempo:3200/metrics",
            ["otel"] = "http://otel:8888/metrics",
            ["prometheus"] = "http://prom:9090/metrics",
        };

        foreach (var (service, uri) in endpoints)
        {
            try
            {
                var result = await new HttpClient().GetAsync(uri);
                if (result.IsSuccessStatusCode)
                    logger.LogInformation($"Service {service} is up and running");
                else
                {
                    logger.LogError($"Service {service} is down");
                    failingTests.Add(service);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Service {service} is down");
                failingTests.Add(service);
            }
        }

        return failingTests;
    }
}