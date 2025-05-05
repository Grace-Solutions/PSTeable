using System;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Creates a new Teable automation
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableAutomation")]
    [OutputType(typeof(TeableAutomation))]
    public class NewTeableAutomation : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to create the automation for
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The name of the automation
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }
        
        /// <summary>
        /// The trigger for the automation
        /// </summary>
        [Parameter(Mandatory = true)]
        public TeableAutomationTrigger Trigger { get; set; }
        
        /// <summary>
        /// The actions for the automation
        /// </summary>
        [Parameter(Mandatory = true)]
        public TeableAutomationAction[] Actions { get; set; }
        
        /// <summary>
        /// Whether the automation is enabled
        /// </summary>
        [Parameter()]
        public SwitchParameter Enabled { get; set; } = true;
        
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
                // Create the request body
                var requestBody = new
                {
                    name = Name,
                    tableId = TableId,
                    trigger = Trigger,
                    actions = Actions,
                    enabled = Enabled.IsPresent
                };
                
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetAutomationsUrl(TableId)))
                {
                    Content = content
                };
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableAutomation>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                // Check the response
                if (response?.Data != null)
                {
                    WriteObject(response.Data);
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new Exception("Failed to create automation"),
                        "CreateAutomationFailed",
                        ErrorCategory.InvalidOperation,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateAutomationFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
