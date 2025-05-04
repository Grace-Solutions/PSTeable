using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable fields
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableField")]
    [OutputType(typeof(TeableField))]
    public class GetTeableField : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to get fields from
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByTable")]
        public string TableId { get; set; }
        
        /// <summary>
        /// The ID of the field to get
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByField")]
        public string FieldId { get; set; }
        
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
                    // Get all fields in a table
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetFieldsUrl(TableId));
                    
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);
                    
                    if (response?.Data != null)
                    {
                        foreach (var field in response.Data)
                        {
                            WriteObject(field);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(FieldId))
                {
                    // Get a specific field
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetFieldUrl(FieldId));
                    
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
                else
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Either TableId or FieldId must be specified"),
                        "MissingParameter",
                        ErrorCategory.InvalidArgument,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetFieldFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


