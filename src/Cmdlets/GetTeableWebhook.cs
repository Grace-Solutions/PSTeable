using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable webhooks
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableWebhook")]
    [OutputType(typeof(TeableWebhook))]
    public class GetTeableWebhook : PSCmdlet
    {
        /// <summary>
        /// The ID of the webhook
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ById")]
        public string WebhookId { get; set; }
        
        /// <summary>
        /// The ID of the table to get webhooks for
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
                    // Get a specific webhook
                    if (string.IsNullOrEmpty(WebhookId))
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException("WebhookId is required"),
                            "MissingWebhookId",
                            ErrorCategory.InvalidArgument,
                            null));
                        return;
                    }
                    
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetWebhookUrl(WebhookId)));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableWebhook>>(
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
                            new ItemNotFoundException($"Webhook {WebhookId} not found"),
                            "WebhookNotFound",
                            ErrorCategory.ObjectNotFound,
                            WebhookId));
                    }
                }
                else // ByTable
                {
                    // Get all webhooks for a table
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetWebhooksUrl(TableId)));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableWebhook>>(
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
                        WriteWarning("No webhooks found");
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetWebhookFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
