using System;
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
    /// Invokes a batch operation on Teable records
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "TeableBatch")]
    [OutputType(typeof(TeableBatchResult))]
    public class InvokeTeableBatch : PSCmdlet
    {
        /// <summary>
        /// The records to process
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public TeableRecord[] Records { get; set; }
        
        /// <summary>
        /// The ID of the table to operate on
        /// </summary>
        [Parameter(Mandatory = true)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The type of operation to perform
        /// </summary>
        [Parameter(Mandatory = true)]
        public TeableBatchOperationType Operation { get; set; }
        
        /// <summary>
        /// The number of records to process in each batch
        /// </summary>
        [Parameter()]
        public int BatchSize { get; set; } = 100;
        
        /// <summary>
        /// The delay between batches in milliseconds
        /// </summary>
        [Parameter()]
        public int BatchDelayMs { get; set; } = 0;
        
        /// <summary>
        /// Whether to continue on error
        /// </summary>
        [Parameter()]
        public SwitchParameter ContinueOnError { get; set; }
        
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
        /// The list of records to process
        /// </summary>
        private List<TeableRecord> _records = new List<TeableRecord>();
        
        /// <summary>
        /// The list of errors that occurred during processing
        /// </summary>
        private List<string> _errors = new List<string>();
        
        /// <summary>
        /// The number of records that were successfully processed
        /// </summary>
        private int _successCount = 0;
        
        /// <summary>
        /// The stopwatch for measuring the duration of the operation
        /// </summary>
        private Stopwatch _stopwatch = new Stopwatch();
        
        /// <summary>
        /// Processes each record from the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Records != null)
            {
                _records.AddRange(Records);
            }
        }
        
        /// <summary>
        /// Processes the cmdlet after all pipeline input has been processed
        /// </summary>
        protected override void EndProcessing()
        {
            try
            {
                // Start the stopwatch
                _stopwatch.Start();
                
                // Process the records in batches
                int totalRecords = _records.Count;
                int totalBatches = (int)Math.Ceiling((double)totalRecords / BatchSize);
                int currentBatch = 0;
                
                for (int i = 0; i < totalRecords; i += BatchSize)
                {
                    currentBatch++;
                    
                    // Get the current batch of records
                    var batch = _records.Skip(i).Take(BatchSize).ToList();
                    
                    // Write progress
                    WriteProgress(new ProgressRecord(
                        1,
                        $"Processing {Operation} batch",
                        $"Batch {currentBatch} of {totalBatches}")
                    {
                        PercentComplete = (int)((double)currentBatch / totalBatches * 100),
                        CurrentOperation = $"Processing {batch.Count} records"
                    });
                    
                    try
                    {
                        // Process the batch
                        ProcessBatch(batch);
                    }
                    catch (Exception ex)
                    {
                        _errors.Add($"Batch {currentBatch} failed: {ex.Message}");
                        
                        if (!ContinueOnError)
                        {
                            throw;
                        }
                    }
                    
                    // Delay between batches if requested
                    if (BatchDelayMs > 0 && currentBatch < totalBatches)
                    {
                        Thread.Sleep(BatchDelayMs);
                    }
                }
                
                // Stop the stopwatch
                _stopwatch.Stop();
                
                // Complete the progress
                WriteProgress(new ProgressRecord(
                    1,
                    $"Processing {Operation} batch",
                    "Completed")
                {
                    PercentComplete = 100,
                    RecordType = ProgressRecordType.Completed
                });
                
                // Return the result
                WriteObject(new TeableBatchResult
                {
                    OperationType = Operation,
                    TotalRecords = totalRecords,
                    SuccessCount = _successCount,
                    FailureCount = totalRecords - _successCount,
                    Duration = _stopwatch.Elapsed,
                    Errors = _errors.ToArray()
                });
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "BatchOperationFailed",
                    ErrorCategory.OperationStopped,
                    null));
            }
        }
        
        /// <summary>
        /// Processes a batch of records
        /// </summary>
        /// <param name="batch">The batch of records to process</param>
        private void ProcessBatch(List<TeableRecord> batch)
        {
            switch (Operation)
            {
                case TeableBatchOperationType.Create:
                    CreateRecords(batch);
                    break;
                case TeableBatchOperationType.Update:
                    UpdateRecords(batch);
                    break;
                case TeableBatchOperationType.Delete:
                    DeleteRecords(batch);
                    break;
                default:
                    throw new ArgumentException($"Unsupported operation type: {Operation}");
            }
        }
        
        /// <summary>
        /// Creates a batch of records
        /// </summary>
        /// <param name="batch">The batch of records to create</param>
        private void CreateRecords(List<TeableRecord> batch)
        {
            // Create the request body
            var records = new List<object>();
            foreach (var record in batch)
            {
                records.Add(new { fields = record.Fields });
            }
            
            var requestBody = new { records };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Create the request
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(TeableUrlBuilder.GetRecordsUrl(TableId)))
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
                _successCount += response.Data.Count;
                
                if (response.Data.Count < batch.Count)
                {
                    _errors.Add($"Only {response.Data.Count} of {batch.Count} records were created");
                }
            }
            else
            {
                _errors.Add("Failed to create records");
            }
        }
        
        /// <summary>
        /// Updates a batch of records
        /// </summary>
        /// <param name="batch">The batch of records to update</param>
        private void UpdateRecords(List<TeableRecord> batch)
        {
            // Process each record individually
            foreach (var record in batch)
            {
                if (string.IsNullOrEmpty(record.Id))
                {
                    _errors.Add("Record ID is required for update operations");
                    continue;
                }
                
                try
                {
                    // Create the request body
                    var requestBody = new { fields = record.Fields };
                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    // Create the request
                    var request = new HttpRequestMessage(
                        new HttpMethod("PATCH"),
                        new Uri(TeableUrlBuilder.GetRecordUrl(TableId, record.Id)))
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
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        _successCount++;
                    }
                    else
                    {
                        _errors.Add($"Failed to update record {record.Id}");
                    }
                }
                catch (Exception ex)
                {
                    _errors.Add($"Failed to update record {record.Id}: {ex.Message}");
                    
                    if (!ContinueOnError)
                    {
                        throw;
                    }
                }
            }
        }
        
        /// <summary>
        /// Deletes a batch of records
        /// </summary>
        /// <param name="batch">The batch of records to delete</param>
        private void DeleteRecords(List<TeableRecord> batch)
        {
            // Get the record IDs
            var recordIds = batch.Select(r => r.Id).Where(id => !string.IsNullOrEmpty(id)).ToList();
            
            if (recordIds.Count == 0)
            {
                _errors.Add("No valid record IDs found for delete operation");
                return;
            }
            
            // Create the request body
            var requestBody = new { recordIds };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Create the request
            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                new Uri(TeableUrlBuilder.GetRecordsUrl(TableId)))
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
            if (response != null && response.IsSuccessStatusCode)
            {
                _successCount += recordIds.Count;
            }
            else
            {
                _errors.Add($"Failed to delete {recordIds.Count} records");
            }
        }
    }
}
