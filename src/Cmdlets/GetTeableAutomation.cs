using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable automations
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableAutomation")]
    [OutputType(typeof(TeableAutomation))]
    public class GetTeableAutomation : PSCmdlet
    {
        /// <summary>
        /// The ID of the automation
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ById")]
        public string AutomationId { get; set; }
        
        /// <summary>
        /// The ID of the table to get automations for
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByTable")]
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
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ParameterSetName == "ById")
                {
                    // Get a specific automation
                    if (string.IsNullOrEmpty(AutomationId))
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException("AutomationId is required"),
                            "MissingAutomationId",
                            ErrorCategory.InvalidArgument,
                            null));
                        return;
                    }
                    
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetAutomationUrl(AutomationId)));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableAutomation>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);
                    
                    if (response?.Data != null)
                    {
                        WriteObject(response.Data);
                    }
                    else
                    {
                        WriteError(new ErrorRecord(
                            new ItemNotFoundException($"Automation {AutomationId} not found"),
                            "AutomationNotFound",
                            ErrorCategory.ObjectNotFound,
                            AutomationId));
                    }
                }
                else // ByTable
                {
                    // Get all automations for a table
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetAutomationsUrl(TableId)));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableAutomation>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);
                    
                    if (response?.Data != null)
                    {
                        WriteObject(response.Data, true);
                    }
                    else
                    {
                        WriteWarning("No automations found");
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetAutomationFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
