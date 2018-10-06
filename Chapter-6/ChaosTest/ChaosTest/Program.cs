using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Chaos.DataStructures;
using System.Linq;
using System.Threading.Tasks;

namespace ChaosTest
{
    internal class Program
    {
        /*
         * This method sets up a 60-minute chaos test with a 30-second cluster stabilization period.
         * The program reacts to test events and stops when it sees StoppedEvent.
         */
        private static void Main(string[] args)
        {
            var clusterConnectionString = "localhost:19000";
            using (var client = new FabricClient(clusterConnectionString))
            {
                var startTimeUtc = DateTime.UtcNow;
                var stabilizationTimeout = TimeSpan.FromSeconds(30.0);
                var timeToRun = TimeSpan.FromMinutes(60.0);
                var maxConcurrentFaults = 3;
                var parameters = new ChaosParameters(
                    stabilizationTimeout,
                    maxConcurrentFaults,
                    true, /* EnableMoveReplicaFault */
                    timeToRun);
                try
                {
                    client.TestManager.StartChaosAsync(parameters).GetAwaiter().GetResult();
                }
                catch (FabricChaosAlreadyRunningException)
                {
                    Console.WriteLine("An instance of Chaos is already running in the cluster.");
                }

                var filter = new ChaosReportFilter(startTimeUtc, DateTime.MaxValue);
                var eventSet = new HashSet<ChaosEvent>(new ChaosEventComparer());
                while (true)
                {
                    var report = client.TestManager.GetChaosReportAsync(filter)
                        .GetAwaiter().GetResult();
                    foreach (var chaosEvent in report.History)
                    {
                        if (eventSet.Add(chaosEvent))
                        {
                            Console.WriteLine(chaosEvent);
                        }
                    }
                    var lastEvent = report.History.LastOrDefault();

                    if (lastEvent is StoppedEvent)
                    {
                        break;
                    }
                    Task.Delay(TimeSpan.FromSeconds(1.0)).GetAwaiter().GetResult();
                }
            }
        }
    }
}
