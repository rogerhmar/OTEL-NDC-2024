using System.Security.Cryptography;

namespace exampleApi.Service;

public class TestingService(ILogger<TestingService> logger)
{
    public async Task<List<string>> Test()
    {
        var failingTests = new List<string>();
        
        var endpoints = new Dictionary<string, string>
        {
            ["grafana"] = "http://localhost:3000/metrics",
            ["loki"] = "http://localhost:3100/metrics",
            ["tempo"] = "http://localhost:3200/metrics",
            ["prometheus"] = "http://localhost:9090/metrics",
            ["dependency1"] = "http://localhost:8080/health",
            ["dependency2"] = "http://localhost:8081/health",
            ["dependency3"] = "http://localhost:8082/health",
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