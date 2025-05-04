using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable view
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableView", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(void))]
    public class RemoveTeableView : PSCmdlet
    {
        /// <summary>
        /// The ID of the view to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string ViewId { get; set; }
        
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
                if (!Force && !ShouldProcess(ViewId, "Remove view"))
                {
                    return;
                }
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    new Uri(TeableUrlBuilder.GetViewUrl(ViewId));
                
                // Send the request
                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (response != null && response.IsSuccessStatusCode)
                {
                    WriteVerbose($"View {ViewId} removed successfully");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveViewFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


