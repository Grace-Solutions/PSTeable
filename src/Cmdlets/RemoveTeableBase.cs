using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable base
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableBase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class RemoveTeableBase : PSCmdlet
    {
        /// <summary>
        /// The ID of the base to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string BaseId { get; set; }
        
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
                if (!Force && !ShouldProcess(BaseId, "Remove base"))
                {
                    return;
                }
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    new Uri(TeableUrlBuilder.GetBaseUrl(BaseId));
                
                // Send the request
                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (response != null && response.IsSuccessStatusCode)
                {
                    WriteVerbose($"Base {BaseId} removed successfully");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveBaseFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


