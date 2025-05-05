using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable automation
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableAutomation", SupportsShouldProcess = true)]
    [OutputType(typeof(void))]
    public class RemoveTeableAutomation : PSCmdlet
    {
        /// <summary>
        /// The ID of the automation to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string AutomationId { get; set; }
        
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
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Confirm the removal
                if (!ShouldProcess(AutomationId, "Remove automation"))
                {
                    return;
                }
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    new Uri(TeableUrlBuilder.GetAutomationUrl(AutomationId)));
                
                // Send the request
                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                // Check the response
                if (response == null || !response.IsSuccessStatusCode)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to remove automation {AutomationId}"),
                        "RemoveAutomationFailed",
                        ErrorCategory.InvalidOperation,
                        AutomationId));
                }
                else
                {
                    WriteVerbose($"Automation {AutomationId} removed successfully");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveAutomationFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
