namespace exampleApi.Service
{
    public class SuperService
    {
        private readonly ILogger<SuperService> _logger;

        public SuperService(ILogger<SuperService> logger)
        {
            _logger = logger;
        }
        public async Task<Message> Dependency1(string nameOfMethod)
        {
            _logger.LogInformation("{name} is being done", nameOfMethod);
            using var _ = Signals.MyActivitySource.StartActivity($"{nameof(Dependency1)}-{nameOfMethod}");
            await ThisNeedsToBeTraced();
            return await Dependency2($"{nameOfMethod}_sub", $"extra {500}ms + ");
        }

        public async Task<Message> Dependency2(string nameOfMethod, string comment = "")
        {
            _logger.LogDebug("{name} is being done with comment '{comment}'", nameOfMethod, comment);
            using var _ = Signals.MyActivitySource.StartActivity($"{nameof(Dependency2)}-{nameOfMethod}");
            var delay = new Random().Next(500);
            
            await Task.Delay(delay);
            
            return new Message($"Dependency added {comment} {delay}ms delay");
        }
        
        /// <summary>
        /// This function does work and should be traced.
        ///
        /// </summary>
        /// <returns></returns>
        public async Task ThisNeedsToBeTraced()
        {
            //using var _ = Signals.MyActivitySource.StartActivity($"{nameof(Dependency1)}-{nameof(ThisNeedsToBeTraced)}", ActivityKind.Internal);
            await Task.Delay(500);
         }
    }
}
