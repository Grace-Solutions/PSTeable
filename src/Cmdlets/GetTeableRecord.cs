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
    /// Gets Teable records
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableRecord")]
    [OutputType(typeof(TeableRecord))]
    public class GetTeableRecord : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to get records from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The ID of the record to get
        /// </summary>
        [Parameter(Position = 1)]
        public string RecordId { get; set; }
        
        /// <summary>
        /// The ID of the view to filter records by
        /// </summary>
        [Parameter()]
        public string ViewId { get; set; }
        
        /// <summary>
        /// The filter to apply to the records
        /// </summary>
        [Parameter()]
        public Hashtable Filter { get; set; }
        
        /// <summary>
        /// The fields to sort by
        /// </summary>
        [Parameter()]
        public string[] SortBy { get; set; }
        
        /// <summary>
        /// Whether to sort in descending order
        /// </summary>
        [Parameter()]
        public SwitchParameter Descending { get; set; }
        
        /// <summary>
        /// The fields to include in the response
        /// </summary>
        [Parameter()]
        public string[] Property { get; set; }
        
        /// <summary>
        /// The maximum number of records to return
        /// </summary>
        [Parameter()]
        public int MaxCount { get; set; } = int.MaxValue;
        
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
                if (string.IsNullOrEmpty(RecordId))
                {
                    // Get all records
                    GetAllRecords();
                }
                else
                {
                    // Get a specific record
                    GetSingleRecord();
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetRecordFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
        
        private void GetAllRecords()
        {
            // Build the filter string
            string filterString = null;
            if (Filter != null && Filter.Count > 0)
            {
                var filterObject = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in Filter)
                {
                    filterObject.Add(entry.Key.ToString(), entry.Value);
                }
                
                filterString = JsonSerializer.Serialize(filterObject);
            }
            
            // Build the sort string
            string sortString = null;
            if (SortBy != null && SortBy.Length > 0)
            {
                var sortObject = new List<Dictionary<string, object>>();
                foreach (var field in SortBy)
                {
                    sortObject.Add(new Dictionary<string, object>
                    {
                        { "field", field },
                        { "order", Descending ? "desc" : "asc" }
                    });
                }
                
                sortString = JsonSerializer.Serialize(sortObject);
            }
            
            // Build the fields string
            string fieldsString = null;
            if (Property != null && Property.Length > 0)
            {
                fieldsString = string.Join(",", Property);
            }
            
            // Get records with pagination
            string pageToken = null;
            int recordCount = 0;
            
            do
            {
                var url = TeableUrlBuilder.GetRecordsUrl(
                    TableId,
                    ViewId,
                    filterString,
                    sortString,
                    fieldsString,
                    100, // Page size
                    pageToken);
                
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (response?.Data != null)
                {
                    foreach (var record in response.Data)
                    {
                        WriteObject(record);
                        recordCount++;
                        
                        if (recordCount >= MaxCount)
                        {
                            return;
                        }
                    }
                    
                    pageToken = response.NextPageToken;
                }
                else
                {
                    pageToken = null;
                }
            }
            while (!string.IsNullOrEmpty(pageToken) && recordCount < MaxCount);
        }
        
        private void GetSingleRecord()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetRecordUrl(TableId, RecordId));
            
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
    }
}



