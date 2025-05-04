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
    /// Creates a new Teable field
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableField")]
    [OutputType(typeof(TeableField))]
    public class NewTeableField : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to create the field in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The name of the field
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }
        
        /// <summary>
        /// The type of the field
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        [ValidateSet("singleLineText", "longText", "number", "checkbox", "singleSelect", "multipleSelect", "date", "dateTime", "attachment", "link", "formula", "rollup", "count", "lookup", "user", "createdTime", "lastModifiedTime", "createdBy", "lastModifiedBy")]
        public string Type { get; set; }
        
        /// <summary>
        /// The options for the field
        /// </summary>
        [Parameter()]
        public Hashtable Options { get; set; }
        
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
                // Convert options to a dictionary
                var optionsDict = new System.Collections.Generic.Dictionary<string, object>();
                if (Options != null)
                {
                    foreach (DictionaryEntry entry in Options)
                    {
                        optionsDict.Add(entry.Key.ToString(), entry.Value);
                    }
                }
                
                // Create the request body
                var body = new
                {
                    name = Name,
                    type = Type,
                    options = optionsDict
                };
                
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetFieldsUrl(TableId))
                {
                    Content = content
                };
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableField>>(
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
                    "CreateFieldFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


