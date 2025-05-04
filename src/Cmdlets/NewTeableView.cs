using System;
using System.Collections;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Creates a new Teable view
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableView")]
    [OutputType(typeof(TeableView))]
    public class NewTeableView : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to create the view in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The name of the view
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }
        
        /// <summary>
        /// The type of the view
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        [ValidateSet("grid", "kanban", "gallery", "gantt", "calendar", "form")]
        public string Type { get; set; }
        
        /// <summary>
        /// The filter for the view
        /// </summary>
        [Parameter()]
        public Hashtable Filter { get; set; }
        
        /// <summary>
        /// The sort for the view
        /// </summary>
        [Parameter()]
        public Hashtable Sort { get; set; }
        
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
                // Convert filter and sort to dictionaries
                var filterDict = new System.Collections.Generic.Dictionary<string, object>();
                if (Filter != null)
                {
                    foreach (DictionaryEntry entry in Filter)
                    {
                        filterDict.Add(entry.Key.ToString(), entry.Value);
                    }
                }
                
                var sortDict = new System.Collections.Generic.Dictionary<string, object>();
                if (Sort != null)
                {
                    foreach (DictionaryEntry entry in Sort)
                    {
                        sortDict.Add(entry.Key.ToString(), entry.Value);
                    }
                }
                
                // Create the request body
                var body = new
                {
                    name = Name,
                    type = Type,
                    filter = filterDict.Count > 0 ? filterDict : null,
                    sort = sortDict.Count > 0 ? sortDict : null
                };
                
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetViewsUrl(TableId))
                {
                    Content = content
                };
                
                // Send the request
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
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateViewFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


