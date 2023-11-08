using System.Diagnostics;
using OpenTelemetry.Trace;

namespace exampleApi
{
    public class SuperService
    {
        public async Task<Message> Dependency1(string nameOfMethod)
        {
            using var _ = Signals.MyActivitySource.StartActivity($"{nameof(Dependency1)}-{nameOfMethod}");
            await ThisNeedsToBeTraced();
            return await Dependency2($"{nameOfMethod}_sub", $"extra {500}ms + ");
        }

        public async Task<Message> Dependency2(string nameOfMethod, string comment = "")
        {
            using var _ = Signals.MyActivitySource.StartActivity($"{nameof(Dependency2)}-{nameOfMethod}",
                ActivityKind.Internal);
            var delay = new Random().Next(500);
            await Task.Delay(delay);
            return new Message($"Dependency added {comment} {delay}ms delay");
        }
        
        /// <summary>
        /// This function does work and should be traced.
        ///
        /// For learning: Add tracing manually, and not by using Tracer.TraceMethod
        /// </summary>
        /// <returns></returns>
        public async Task ThisNeedsToBeTraced()
        {
            using var _ =
                Signals.MyActivitySource.CreateActivity($"{nameof(Dependency1)}-{nameof(ThisNeedsToBeTraced)}", ActivityKind.Internal);
            await Task.Delay(500);
         }
    }
}
