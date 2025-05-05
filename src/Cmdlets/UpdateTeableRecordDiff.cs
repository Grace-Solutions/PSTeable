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
    /// Updates a Teable record with only the changed fields
    /// </summary>
    [Cmdlet(VerbsData.Update, "TeableRecordDiff")]
    [OutputType(typeof(TeableRecord))]
    public class UpdateTeableRecordDiff : PSCmdlet
    {
        /// <summary>
        /// The record to update
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public TeableRecord Record { get; set; }
        
        /// <summary>
        /// The ID of the table containing the record
        /// </summary>
        [Parameter(Mandatory = true)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The original record to compare against
        /// </summary>
        [Parameter(Mandatory = true)]
        public TeableRecord OriginalRecord { get; set; }
        
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
                // Check if the record has an ID
                if (string.IsNullOrEmpty(Record.Id))
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Record ID is required"),
                        "MissingRecordId",
                        ErrorCategory.InvalidArgument,
                        Record));
                    return;
                }
                
                // Check if the original record has the same ID
                if (OriginalRecord.Id != Record.Id)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Original record ID must match the record ID"),
                        "RecordIdMismatch",
                        ErrorCategory.InvalidArgument,
                        OriginalRecord));
                    return;
                }
                
                // Get the changed fields
                var changedFields = GetChangedFields(Record.Fields, OriginalRecord.Fields);
                
                // If there are no changes, return the original record
                if (changedFields.Count == 0)
                {
                    WriteVerbose($"No changes detected for record {Record.Id}");
                    WriteObject(Record);
                    return;
                }
                
                // Create the request body
                var requestBody = new { fields = changedFields };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Create the request
                var request = new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    new Uri(TeableUrlBuilder.GetRecordUrl(TableId, Record.Id)))
                {
                    Content = content
                };
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableRecord>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                // Check the response
                if (response?.Data != null)
                {
                    WriteVerbose($"Updated {changedFields.Count} fields in record {Record.Id}");
                    WriteObject(response.Data);
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to update record {Record.Id}"),
                        "UpdateRecordFailed",
                        ErrorCategory.InvalidOperation,
                        Record));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "UpdateRecordDiffFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Gets the fields that have changed between two records
        /// </summary>
        /// <param name="newFields">The new fields</param>
        /// <param name="originalFields">The original fields</param>
        /// <returns>A dictionary of changed fields</returns>
        private Dictionary<string, object> GetChangedFields(Dictionary<string, object> newFields, Dictionary<string, object> originalFields)
        {
            var changedFields = new Dictionary<string, object>();
            
            // Check for fields that have been added or modified
            foreach (var field in newFields)
            {
                if (!originalFields.TryGetValue(field.Key, out var originalValue) ||
                    !JsonEquals(field.Value, originalValue))
                {
                    changedFields[field.Key] = field.Value;
                }
            }
            
            return changedFields;
        }
        
        /// <summary>
        /// Compares two objects for equality using JSON serialization
        /// </summary>
        /// <param name="a">The first object</param>
        /// <param name="b">The second object</param>
        /// <returns>True if the objects are equal, false otherwise</returns>
        private bool JsonEquals(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            
            if (a == null || b == null)
            {
                return false;
            }
            
            // Serialize both objects to JSON
            string jsonA = JsonSerializer.Serialize(a);
            string jsonB = JsonSerializer.Serialize(b);
            
            // Compare the JSON strings
            return jsonA == jsonB;
        }
    }
}
