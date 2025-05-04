using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable table
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableTable", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class RemoveTeableTable : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }

        /// <summary>
        /// Whether to respect rate limits
        /// </summary>
        [Parameter()]
        public SwitchParameter RespectRateLimit { get; set; }

        /// <summary>
        /// The delay to use when rate limited
        /// </summary>
        [Parameter()]
        public TimeSpan RateLimitDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Whether to force the operation without confirmation
        /// </summary>
        [Parameter()]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Confirm the operation
                if (!Force && !ShouldProcess(TableId, "Remove table"))
                {
                    return;
                }

                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    new Uri(TeableUrlBuilder.GetTableUrl(TableId)));

                // Send the request
                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response != null && response.IsSuccessStatusCode)
                {
                    WriteVerbose($"Table {TableId} removed successfully");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveTableFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}




