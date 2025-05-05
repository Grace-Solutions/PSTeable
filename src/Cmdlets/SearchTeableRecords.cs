using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Searches for records in a Teable table
    /// </summary>
    [Cmdlet(VerbsCommon.Search, "TeableRecords")]
    [OutputType(typeof(TeableRecord[]))]
    public class SearchTeableRecords : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to search in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Table")]
        public string TableId { get; set; }
        
        /// <summary>
        /// The ID of the view to search in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "View")]
        public string ViewId { get; set; }
        
        /// <summary>
        /// The search query
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Query { get; set; }
        
        /// <summary>
        /// The fields to search in
        /// </summary>
        [Parameter()]
        public string[] Fields { get; set; }
        
        /// <summary>
        /// The filter to apply
        /// </summary>
        [Parameter()]
        public TeableFilter Filter { get; set; }
        
        /// <summary>
        /// The sort to apply
        /// </summary>
        [Parameter()]
        public TeableSort Sort { get; set; }
        
        /// <summary>
        /// The maximum number of records to return
        /// </summary>
        [Parameter()]
        public int Limit { get; set; } = 100;
        
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
                // Build the search request
                var searchRequest = new
                {
                    query = Query,
                    fields = Fields,
                    filter = Filter != null ? JsonDocument.Parse(Filter.ToJson()).RootElement : null,
                    sort = Sort != null ? JsonDocument.Parse(Sort.ToJson()).RootElement : null,
                    limit = Limit
                };
                
                // Serialize the search request
                var json = JsonSerializer.Serialize(searchRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(ParameterSetName == "Table"
                        ? TeableUrlBuilder.GetTableSearchUrl(TableId)
                        : TeableUrlBuilder.GetViewSearchUrl(ViewId)))
                {
                    Content = content
                };
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
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
                    WriteWarning("No records found");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SearchRecordsFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}

