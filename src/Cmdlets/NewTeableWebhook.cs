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
    /// Creates a new Teable webhook
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableWebhook")]
    [OutputType(typeof(TeableWebhook))]
    public class NewTeableWebhook : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to create the webhook for
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The name of the webhook
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }
        
        /// <summary>
        /// The URL to send webhook events to
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        public string Url { get; set; }
        
        /// <summary>
        /// The event types to trigger the webhook
        /// </summary>
        [Parameter(Mandatory = true)]
        public TeableWebhookEventType[] EventTypes { get; set; }
        
        /// <summary>
        /// Whether the webhook is enabled
        /// </summary>
        [Parameter()]
        public SwitchParameter Enabled { get; set; } = true;
        
        /// <summary>
        /// The secret used to sign webhook payloads
        /// </summary>
        [Parameter()]
        public string Secret { get; set; }
        
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
                    url = Url,
                    tableId = TableId,
                    eventTypes = EventTypes,
                    enabled = Enabled.IsPresent,
                    secret = Secret
                };
                
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetWebhooksUrl(TableId)))
                {
                    Content = content
                };
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableWebhook>>(
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
                        new Exception("Failed to create webhook"),
                        "CreateWebhookFailed",
                        ErrorCategory.InvalidOperation,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateWebhookFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
