using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Synchronizes data between two Teable tables
    /// </summary>
    [Cmdlet(VerbsData.Sync, "TeableData")]
    [OutputType(typeof(PSObject))]
    public class SyncTeableData : PSCmdlet
    {
        /// <summary>
        /// The ID of the source table
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string SourceTableId { get; set; }
        
        /// <summary>
        /// The ID of the target table
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string TargetTableId { get; set; }
        
        /// <summary>
        /// The field mapping between source and target tables
        /// </summary>
        [Parameter()]
        public Hashtable FieldMapping { get; set; }
        
        /// <summary>
        /// The field to use as the key for matching records
        /// </summary>
        [Parameter(Mandatory = true)]
        public string KeyField { get; set; }
        
        /// <summary>
        /// Whether to create records that don't exist in the target
        /// </summary>
        [Parameter()]
        public SwitchParameter CreateMissing { get; set; } = true;
        
        /// <summary>
        /// Whether to update records that exist in the target
        /// </summary>
        [Parameter()]
        public SwitchParameter UpdateExisting { get; set; } = true;
        
        /// <summary>
        /// Whether to delete records in the target that don't exist in the source
        /// </summary>
        [Parameter()]
        public SwitchParameter DeleteExtra { get; set; }
        
        /// <summary>
        /// The filter to apply to the source records
        /// </summary>
        [Parameter()]
        public TeableFilter SourceFilter { get; set; }
        
        /// <summary>
        /// The filter to apply to the target records
        /// </summary>
        [Parameter()]
        public TeableFilter TargetFilter { get; set; }
        
        /// <summary>
        /// The batch size for operations
        /// </summary>
        [Parameter()]
        public int BatchSize { get; set; } = 100;
        
        /// <summary>
        /// The delay between batches in milliseconds
        /// </summary>
        [Parameter()]
        public int BatchDelayMs { get; set; } = 0;
        
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
        /// Whether to perform a bidirectional sync
        /// </summary>
        [Parameter()]
        public SwitchParameter Bidirectional { get; set; }
        
        /// <summary>
        /// The timestamp to use for bidirectional sync
        /// </summary>
        [Parameter()]
        public DateTime? SyncSince { get; set; }
        
        /// <summary>
        /// Whether to perform a dry run
        /// </summary>
        [Parameter()]
        public SwitchParameter WhatIf { get; set; }
        
        /// <summary>
        /// The stopwatch for measuring the duration of the operation
        /// </summary>
        private Stopwatch _stopwatch = new Stopwatch();
        
        /// <summary>
        /// The statistics for the sync operation
        /// </summary>
        private Dictionary<string, int> _stats = new Dictionary<string, int>
        {
            { "SourceRecords", 0 },
            { "TargetRecords", 0 },
            { "Created", 0 },
            { "Updated", 0 },
            { "Deleted", 0 },
            { "Unchanged", 0 },
            { "Failed", 0 }
        };
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Start the stopwatch
                _stopwatch.Start();
                
                // Get the field mapping
                var fieldMap = GetFieldMapping();
                
                if (Bidirectional && SyncSince.HasValue)
                {
                    // Perform a bidirectional sync based on changes
                    SyncBidirectional(fieldMap);
                }
                else
                {
                    // Perform a one-way sync
                    SyncOneWay(fieldMap);
                }
                
                // Stop the stopwatch
                _stopwatch.Stop();
                
                // Return the results
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("Duration", _stopwatch.Elapsed));
                result.Properties.Add(new PSNoteProperty("SourceRecords", _stats["SourceRecords"]));
                result.Properties.Add(new PSNoteProperty("TargetRecords", _stats["TargetRecords"]));
                result.Properties.Add(new PSNoteProperty("Created", _stats["Created"]));
                result.Properties.Add(new PSNoteProperty("Updated", _stats["Updated"]));
                result.Properties.Add(new PSNoteProperty("Deleted", _stats["Deleted"]));
                result.Properties.Add(new PSNoteProperty("Unchanged", _stats["Unchanged"]));
                result.Properties.Add(new PSNoteProperty("Failed", _stats["Failed"]));
                
                WriteObject(result);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SyncDataFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Gets the field mapping between source and target tables
        /// </summary>
        /// <returns>The field mapping</returns>
        private Dictionary<string, string> GetFieldMapping()
        {
            var mapping = new Dictionary<string, string>();
            
            if (FieldMapping != null)
            {
                // Use the provided field mapping
                foreach (var key in FieldMapping.Keys)
                {
                    mapping[key.ToString()] = FieldMapping[key].ToString();
                }
            }
            else
            {
                // Get the fields from both tables
                var sourceFields = GetTableFields(SourceTableId);
                var targetFields = GetTableFields(TargetTableId);
                
                // Create a mapping based on field names
                foreach (var sourceField in sourceFields)
                {
                    var targetField = targetFields.FirstOrDefault(f => f.Name == sourceField.Name);
                    if (targetField != null)
                    {
                        mapping[sourceField.Id] = targetField.Id;
                    }
                }
            }
            
            return mapping;
        }
        
        /// <summary>
        /// Gets the fields for a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <returns>The fields</returns>
        private List<TeableField> GetTableFields(string tableId)
        {
            // Create the request
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetFieldsUrl(tableId)));
            
            // Send the request
            var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                request,
                this,
                RespectRateLimit,
                RateLimitDelay);
            
            // Check the response
            if (response?.Data != null)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Failed to get fields for table {tableId}");
            }
        }
        
        /// <summary>
        /// Performs a one-way sync from source to target
        /// </summary>
        /// <param name="fieldMap">The field mapping</param>
        private void SyncOneWay(Dictionary<string, string> fieldMap)
        {
            // Get the source records
            var sourceRecords = GetRecords(SourceTableId, SourceFilter);
            _stats["SourceRecords"] = sourceRecords.Count;
            
            // Get the target records
            var targetRecords = GetRecords(TargetTableId, TargetFilter);
            _stats["TargetRecords"] = targetRecords.Count;
            
            // Create a lookup of target records by key field
            var targetLookup = new Dictionary<string, TeableRecord>();
            foreach (var record in targetRecords)
            {
                if (record.Fields.TryGetValue(KeyField, out var keyValue) && keyValue != null)
                {
                    targetLookup[keyValue.ToString()] = record;
                }
            }
            
            // Process the source records
            var recordsToCreate = new List<TeableRecord>();
            var recordsToUpdate = new List<TeableRecord>();
            
            foreach (var sourceRecord in sourceRecords)
            {
                if (!sourceRecord.Fields.TryGetValue(KeyField, out var keyValue) || keyValue == null)
                {
                    WriteWarning($"Source record {sourceRecord.Id} is missing the key field {KeyField}");
                    continue;
                }
                
                string key = keyValue.ToString();
                
                if (targetLookup.TryGetValue(key, out var targetRecord))
                {
                    // Record exists in target, check if it needs to be updated
                    if (UpdateExisting)
                    {
                        var mappedRecord = MapRecord(sourceRecord, fieldMap);
                        mappedRecord.Id = targetRecord.Id;
                        
                        // Check if the record has changed
                        bool hasChanged = false;
                        foreach (var field in mappedRecord.Fields)
                        {
                            if (!targetRecord.Fields.TryGetValue(field.Key, out var targetValue) ||
                                !JsonEquals(field.Value, targetValue))
                            {
                                hasChanged = true;
                                break;
                            }
                        }
                        
                        if (hasChanged)
                        {
                            recordsToUpdate.Add(mappedRecord);
                        }
                        else
                        {
                            _stats["Unchanged"]++;
                        }
                    }
                    
                    // Remove the record from the target lookup
                    targetLookup.Remove(key);
                }
                else
                {
                    // Record doesn't exist in target, create it
                    if (CreateMissing)
                    {
                        var mappedRecord = MapRecord(sourceRecord, fieldMap);
                        recordsToCreate.Add(mappedRecord);
                    }
                }
            }
            
            // Process records to create
            if (recordsToCreate.Count > 0)
            {
                WriteVerbose($"Creating {recordsToCreate.Count} records");
                
                if (!WhatIf)
                {
                    CreateRecords(TargetTableId, recordsToCreate);
                }
                
                _stats["Created"] = recordsToCreate.Count;
            }
            
            // Process records to update
            if (recordsToUpdate.Count > 0)
            {
                WriteVerbose($"Updating {recordsToUpdate.Count} records");
                
                if (!WhatIf)
                {
                    UpdateRecords(TargetTableId, recordsToUpdate);
                }
                
                _stats["Updated"] = recordsToUpdate.Count;
            }
            
            // Process records to delete
            if (DeleteExtra && targetLookup.Count > 0)
            {
                var recordsToDelete = targetLookup.Values.ToList();
                
                WriteVerbose($"Deleting {recordsToDelete.Count} records");
                
                if (!WhatIf)
                {
                    DeleteRecords(TargetTableId, recordsToDelete);
                }
                
                _stats["Deleted"] = recordsToDelete.Count;
            }
        }
        
        /// <summary>
        /// Performs a bidirectional sync between source and target
        /// </summary>
        /// <param name="fieldMap">The field mapping</param>
        private void SyncBidirectional(Dictionary<string, string> fieldMap)
        {
            if (!SyncSince.HasValue)
            {
                throw new ArgumentException("SyncSince is required for bidirectional sync");
            }
            
            // Create a reverse field map
            var reverseFieldMap = new Dictionary<string, string>();
            foreach (var kvp in fieldMap)
            {
                reverseFieldMap[kvp.Value] = kvp.Key;
            }
            
            // Get changes from the source table
            var sourceChanges = GetChanges(SourceTableId, SyncSince.Value);
            
            // Get changes from the target table
            var targetChanges = GetChanges(TargetTableId, SyncSince.Value);
            
            // Process source changes
            foreach (var change in sourceChanges)
            {
                if (change.Type == TeableChangeType.Create || change.Type == TeableChangeType.Update)
                {
                    // Map the record to the target schema
                    var mappedRecord = MapRecord(change.Record, fieldMap);
                    
                    // Check if the record exists in the target
                    var targetRecord = GetRecordByKeyField(TargetTableId, KeyField, mappedRecord.Fields[KeyField]);
                    
                    if (targetRecord != null)
                    {
                        // Record exists, update it
                        mappedRecord.Id = targetRecord.Id;
                        
                        if (!WhatIf)
                        {
                            UpdateRecord(TargetTableId, mappedRecord);
                        }
                        
                        _stats["Updated"]++;
                    }
                    else
                    {
                        // Record doesn't exist, create it
                        if (!WhatIf)
                        {
                            CreateRecord(TargetTableId, mappedRecord);
                        }
                        
                        _stats["Created"]++;
                    }
                }
                else if (change.Type == TeableChangeType.Delete)
                {
                    // Find the record in the target by key field
                    var targetRecord = GetRecordByKeyField(TargetTableId, KeyField, change.Record.Fields[KeyField]);
                    
                    if (targetRecord != null)
                    {
                        // Delete the record
                        if (!WhatIf)
                        {
                            DeleteRecord(TargetTableId, targetRecord.Id);
                        }
                        
                        _stats["Deleted"]++;
                    }
                }
            }
            
            // Process target changes
            foreach (var change in targetChanges)
            {
                if (change.Type == TeableChangeType.Create || change.Type == TeableChangeType.Update)
                {
                    // Map the record to the source schema
                    var mappedRecord = MapRecord(change.Record, reverseFieldMap);
                    
                    // Check if the record exists in the source
                    var sourceRecord = GetRecordByKeyField(SourceTableId, KeyField, mappedRecord.Fields[KeyField]);
                    
                    if (sourceRecord != null)
                    {
                        // Record exists, update it
                        mappedRecord.Id = sourceRecord.Id;
                        
                        if (!WhatIf)
                        {
                            UpdateRecord(SourceTableId, mappedRecord);
                        }
                        
                        _stats["Updated"]++;
                    }
                    else
                    {
                        // Record doesn't exist, create it
                        if (!WhatIf)
                        {
                            CreateRecord(SourceTableId, mappedRecord);
                        }
                        
                        _stats["Created"]++;
                    }
                }
                else if (change.Type == TeableChangeType.Delete)
                {
                    // Find the record in the source by key field
                    var sourceRecord = GetRecordByKeyField(SourceTableId, KeyField, change.Record.Fields[KeyField]);
                    
                    if (sourceRecord != null)
                    {
                        // Delete the record
                        if (!WhatIf)
                        {
                            DeleteRecord(SourceTableId, sourceRecord.Id);
                        }
                        
                        _stats["Deleted"]++;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets records from a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="filter">Optional filter</param>
        /// <returns>The records</returns>
        private List<TeableRecord> GetRecords(string tableId, TeableFilter filter = null)
        {
            var records = new List<TeableRecord>();
            string pageToken = null;
            
            do
            {
                // Build the URL
                string url = TeableUrlBuilder.GetRecordsUrl(
                    tableId,
                    null,
                    filter?.ToQueryString(),
                    null,
                    null,
                    100, // Page size
                    pageToken);
                
                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                // Check the response
                if (response?.Data != null)
                {
                    records.AddRange(response.Data);
                    pageToken = response.NextPageToken;
                }
                else
                {
                    pageToken = null;
                }
            }
            while (!string.IsNullOrEmpty(pageToken));
            
            return records;
        }
        
        /// <summary>
        /// Gets changes to records in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="since">The timestamp to get changes since</param>
        /// <returns>The changes</returns>
        private List<TeableChange> GetChanges(string tableId, DateTime since)
        {
            // Build the query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "since", since.ToUniversalTime().ToString("o") }
            };
            
            // Create the request
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetTableChangesUrl(tableId, queryParams)));
            
            // Send the request
            var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableChange>>(
                request,
                this,
                RespectRateLimit,
                RateLimitDelay);
            
            // Check the response
            if (response?.Data != null)
            {
                return response.Data;
            }
            else
            {
                return new List<TeableChange>();
            }
        }
        
        /// <summary>
        /// Gets a record by its key field value
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="keyField">The key field</param>
        /// <param name="keyValue">The key value</param>
        /// <returns>The record, or null if not found</returns>
        private TeableRecord GetRecordByKeyField(string tableId, string keyField, object keyValue)
        {
            // Create a filter for the key field
            var filter = new TeableFilter(keyField, TeableFilterOperator.Equal, keyValue);
            
            // Get the records
            var records = GetRecords(tableId, filter);
            
            // Return the first record, or null if none found
            return records.FirstOrDefault();
        }
        
        /// <summary>
        /// Maps a record from one schema to another
        /// </summary>
        /// <param name="record">The record to map</param>
        /// <param name="fieldMap">The field mapping</param>
        /// <returns>The mapped record</returns>
        private TeableRecord MapRecord(TeableRecord record, Dictionary<string, string> fieldMap)
        {
            var mappedRecord = new TeableRecord
            {
                Fields = new Dictionary<string, object>()
            };
            
            foreach (var field in record.Fields)
            {
                if (fieldMap.TryGetValue(field.Key, out var targetField))
                {
                    mappedRecord.Fields[targetField] = field.Value;
                }
            }
            
            return mappedRecord;
        }
        
        /// <summary>
        /// Creates records in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="records">The records to create</param>
        private void CreateRecords(string tableId, List<TeableRecord> records)
        {
            // Process the records in batches
            for (int i = 0; i < records.Count; i += BatchSize)
            {
                var batch = records.Skip(i).Take(BatchSize).ToList();
                
                try
                {
                    // Create the request body
                    var requestBody = new
                    {
                        records = batch.Select(r => new { fields = r.Fields }).ToList()
                    };
                    
                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    // Create the request
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        new Uri(TeableUrlBuilder.GetRecordsUrl(tableId)))
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
                    if (response?.Data == null || response.Data.Count < batch.Count)
                    {
                        _stats["Failed"] += batch.Count - (response?.Data?.Count ?? 0);
                    }
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to create batch: {ex.Message}");
                    _stats["Failed"] += batch.Count;
                }
                
                // Delay between batches if requested
                if (BatchDelayMs > 0 && i + BatchSize < records.Count)
                {
                    Thread.Sleep(BatchDelayMs);
                }
            }
        }
        
        /// <summary>
        /// Updates records in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="records">The records to update</param>
        private void UpdateRecords(string tableId, List<TeableRecord> records)
        {
            // Process the records in batches
            for (int i = 0; i < records.Count; i += BatchSize)
            {
                var batch = records.Skip(i).Take(BatchSize).ToList();
                
                foreach (var record in batch)
                {
                    try
                    {
                        UpdateRecord(tableId, record);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Failed to update record {record.Id}: {ex.Message}");
                        _stats["Failed"]++;
                    }
                }
                
                // Delay between batches if requested
                if (BatchDelayMs > 0 && i + BatchSize < records.Count)
                {
                    Thread.Sleep(BatchDelayMs);
                }
            }
        }
        
        /// <summary>
        /// Updates a record in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="record">The record to update</param>
        private void UpdateRecord(string tableId, TeableRecord record)
        {
            // Create the request body
            var requestBody = new { fields = record.Fields };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Create the request
            var request = new HttpRequestMessage(
                new HttpMethod("PATCH"),
                new Uri(TeableUrlBuilder.GetRecordUrl(tableId, record.Id)))
            {
                Content = content
            };
            
            // Send the request
            using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                request,
                this,
                RespectRateLimit,
                RateLimitDelay);
            
            // Check the response
            if (response == null || !response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to update record {record.Id}");
            }
        }
        
        /// <summary>
        /// Creates a record in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="record">The record to create</param>
        /// <returns>The created record</returns>
        private TeableRecord CreateRecord(string tableId, TeableRecord record)
        {
            // Create the request body
            var requestBody = new { fields = record.Fields };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Create the request
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(TeableUrlBuilder.GetRecordsUrl(tableId)))
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
            if (response?.Data == null)
            {
                throw new Exception("Failed to create record");
            }
            
            return response.Data;
        }
        
        /// <summary>
        /// Deletes records from a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="records">The records to delete</param>
        private void DeleteRecords(string tableId, List<TeableRecord> records)
        {
            // Process the records in batches
            for (int i = 0; i < records.Count; i += BatchSize)
            {
                var batch = records.Skip(i).Take(BatchSize).ToList();
                
                try
                {
                    // Get the record IDs
                    var recordIds = batch.Select(r => r.Id).ToList();
                    
                    // Create the request body
                    var requestBody = new { recordIds };
                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    // Create the request
                    var request = new HttpRequestMessage(
                        HttpMethod.Delete,
                        new Uri(TeableUrlBuilder.GetRecordsUrl(tableId)))
                    {
                        Content = content
                    };
                    
                    // Send the request
                    using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);
                    
                    // Check the response
                    if (response == null || !response.IsSuccessStatusCode)
                    {
                        _stats["Failed"] += batch.Count;
                    }
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to delete batch: {ex.Message}");
                    _stats["Failed"] += batch.Count;
                }
                
                // Delay between batches if requested
                if (BatchDelayMs > 0 && i + BatchSize < records.Count)
                {
                    Thread.Sleep(BatchDelayMs);
                }
            }
        }
        
        /// <summary>
        /// Deletes a record from a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="recordId">The ID of the record to delete</param>
        private void DeleteRecord(string tableId, string recordId)
        {
            // Create the request
            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                new Uri(TeableUrlBuilder.GetRecordUrl(tableId, recordId)));
            
            // Send the request
            using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                request,
                this,
                RespectRateLimit,
                RateLimitDelay);
            
            // Check the response
            if (response == null || !response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete record {recordId}");
            }
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

