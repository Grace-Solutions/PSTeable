using System;
using System.Collections;
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
    /// Creates a new Teable record
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableRecord")]
    [OutputType(typeof(TeableRecord))]
    public class NewTeableRecord : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to create the record in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The fields of the record
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public Hashtable Fields { get; set; }
        
        /// <summary>
        /// The record payload to create
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByPayload", ValueFromPipeline = true)]
        public TeableRecordPayload RecordPayload { get; set; }
        
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
                object body;
                
                if (RecordPayload != null)
                {
                    body = RecordPayload;
                }
                else
                {
                    // Convert fields to a dictionary
                    var fieldsDict = new Dictionary<string, object>();
                    foreach (DictionaryEntry entry in Fields)
                    {
                        fieldsDict.Add(entry.Key.ToString(), entry.Value);
                    }
                    
                    body = new TeableRecordPayload { Fields = fieldsDict };
                }
                
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetRecordsUrl(TableId))
                {
                    Content = content
                };
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableRecord>>(
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
                    "CreateRecordFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


