using System.Diagnostics;

namespace exampleApi.Service
{
    public class SuperService(ActivitySource activitySource)
    {
        public async Task<Message> InternalDependency1(string nameOfMethod)
        {
            // logger.LogInformation("{name} is starting done", nameOfMethod);
            using (activitySource.StartActivity($"{nameof(InternalDependency1)}-{nameOfMethod}")) // Start span
            {
                await ThisNeedsToBeTraced();
                return await InternalDependency2($"{nameOfMethod}_sub", $"extra {500}ms + ");
            } // End span (when object is disposed)
        }

        public async Task<Message> InternalDependency2(string nameOfMethod, string comment = "")
        {
            // logger.LogDebug("{name} is being done with comment '{comment}'", nameOfMethod, comment);
            using var activity = activitySource.StartActivity($"{nameof(InternalDependency2)}-{nameOfMethod}");
            var delay = new Random().Next(500);
            activity?.SetTag("delay", delay);
            activity?.AddEvent(new ActivityEvent("Processing starting"));
            // Do some processing
            await Task.Delay(delay);
            activity?.AddEvent(new ActivityEvent("Processing completed"));
            
            // Call dependency1 (external)
            await new HttpClient().GetAsync(new Uri("http://localhost:8080/slow"));
            
            return new Message($"Dependency added {comment} {delay}ms delay and got reply from external dependency1");
        }
        
        /// <summary>
        /// This function does work and should be traced.
        ///
        /// </summary>
        /// <returns></returns>
        public async Task ThisNeedsToBeTraced()
        {
            using var _ = activitySource.CreateActivity($"{nameof(InternalDependency1)}-{nameof(ThisNeedsToBeTraced)}", ActivityKind.Internal);
            await Task.Delay(800);
         }
    }
}
