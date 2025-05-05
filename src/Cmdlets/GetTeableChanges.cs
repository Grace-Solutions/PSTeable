using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets changes to records in a Teable table
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableChanges")]
    [OutputType(typeof(TeableChange[]))]
    public class GetTeableChanges : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to get changes for
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The timestamp to get changes since
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public DateTime Since { get; set; }
        
        /// <summary>
        /// The types of changes to include
        /// </summary>
        [Parameter()]
        public TeableChangeType[] ChangeTypes { get; set; }
        
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
                // Build the query parameters
                var queryParams = new Dictionary<string, string>
                {
                    { "since", Since.ToUniversalTime().ToString("o") }
                };
                
                if (ChangeTypes != null && ChangeTypes.Length > 0)
                {
                    queryParams.Add("types", string.Join(",", ChangeTypes));
                }
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(TeableUrlBuilder.GetTableChangesUrl(TableId, queryParams)));
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableChange>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                // Check the response
                if (response?.Data != null)
                {
                    WriteObject(response.Data, true);
                }
                else
                {
                    WriteWarning("No changes found");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetChangesFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
