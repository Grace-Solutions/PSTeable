using System;
using PSTeable.Utils;
using System.Diagnostics;
using System.Management.Automation;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Measures the performance of a Teable operation
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Measure, "TeableOperation")]
    [OutputType(typeof(PSObject))]
    public class MeasureTeableOperation : PSCmdlet
    {
        /// <summary>
        /// The script block to measure
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock ScriptBlock { get; set; }

        /// <summary>
        /// The number of iterations to perform
        /// </summary>
        [Parameter()]
        public int Iterations { get; set; } = 1;

        /// <summary>
        /// Whether to warm up before measuring
        /// </summary>
        [Parameter()]
        public SwitchParameter WarmUp { get; set; }

        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Warm up if requested
                if (WarmUp)
                {
                    Logger.Verbose(this, "Warming up...");
                    ScriptBlock.InvokeWithContext(null, null);
                }

                // Measure the operation
                var stopwatch = new Stopwatch();
                var results = new TimeSpan[Iterations];

                for (int i = 0; i < Iterations; i++)
                {
                    Logger.Verbose(this, $"Iteration {i + 1} of {Iterations}...");

                    stopwatch.Restart();
                    ScriptBlock.InvokeWithContext(null, null);
                    stopwatch.Stop();

                    results[i] = stopwatch.Elapsed;
                }

                // Calculate statistics
                var totalTime = TimeSpan.Zero;
                var minTime = TimeSpan.MaxValue;
                var maxTime = TimeSpan.MinValue;

                foreach (var time in results)
                {
                    totalTime += time;

                    if (time < minTime)
                    {
                        minTime = time;
                    }

                    if (time > maxTime)
                    {
                        maxTime = time;
                    }
                }

                var averageTime = TimeSpan.FromTicks(totalTime.Ticks / Iterations);

                // Return the results
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("Iterations", Iterations));
                result.Properties.Add(new PSNoteProperty("TotalTime", totalTime));
                result.Properties.Add(new PSNoteProperty("AverageTime", averageTime));
                result.Properties.Add(new PSNoteProperty("MinTime", minTime));
                result.Properties.Add(new PSNoteProperty("MaxTime", maxTime));

                WriteObject(result);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "MeasureOperationFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}

