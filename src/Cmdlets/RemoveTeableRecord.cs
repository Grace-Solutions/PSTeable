using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable record
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(void))]
    public class RemoveTeableRecord : PSCmdlet
    {
        /// <summary>
        /// The ID of the table that contains the record
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The ID of the record to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string RecordId { get; set; }
        
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
                if (!Force && !ShouldProcess(RecordId, "Remove record"))
                {
                    return;
                }
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    new Uri(TeableUrlBuilder.GetRecordUrl(TableId, RecordId));
                
                // Send the request
                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (response != null && response.IsSuccessStatusCode)
                {
                    WriteVerbose($"Record {RecordId} removed successfully");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveRecordFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


