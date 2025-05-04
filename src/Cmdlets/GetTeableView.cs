using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable views
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableView")]
    [OutputType(typeof(TeableView))]
    public class GetTeableView : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to get views from
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByTable")]
        public string TableId { get; set; }
        
        /// <summary>
        /// The ID of the view to get
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByView")]
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
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (!string.IsNullOrEmpty(TableId))
                {
                    // Get all views in a table
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetViewsUrl(TableId));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableView>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);
                    
                    if (response?.Data != null)
                    {
                        foreach (var view in response.Data)
                        {
                            WriteObject(view);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(ViewId))
                {
                    // Get a specific view
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetViewUrl(ViewId));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableView>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);
                    
                    if (response?.Data != null)
                    {
                        WriteObject(response.Data);
                    }
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Either TableId or ViewId must be specified"),
                        "MissingParameter",
                        ErrorCategory.InvalidArgument,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetViewFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


