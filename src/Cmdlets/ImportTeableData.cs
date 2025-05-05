using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Import format for Teable data
    /// </summary>
    public enum TeableImportFormat
    {
        /// <summary>
        /// JSON format
        /// </summary>
        Json,

        /// <summary>
        /// CSV format
        /// </summary>
        Csv
    }

    /// <summary>
    /// Imports data into a Teable table
    /// </summary>
    [Cmdlet(VerbsData.Import, "TeableData")]
    [OutputType(typeof(TeableRecord[]))]
    public class ImportTeableData : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to import data into
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }

        /// <summary>
        /// The path to the data file
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "File")]
        public string Path { get; set; }

        /// <summary>
        /// The records to import
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Records")]
        public TeableRecord[] Records { get; set; }

        /// <summary>
        /// The format of the data file
        /// </summary>
        [Parameter(ParameterSetName = "File")]
        public TeableImportFormat Format { get; set; } = TeableImportFormat.Json;

        /// <summary>
        /// The delimiter used in CSV files
        /// </summary>
        [Parameter(ParameterSetName = "File")]
        public string Delimiter { get; set; } = ",";

        /// <summary>
        /// Whether the CSV file has a header row
        /// </summary>
        [Parameter(ParameterSetName = "File")]
        public SwitchParameter NoHeader { get; set; }

        /// <summary>
        /// The field mapping for CSV import
        /// </summary>
        [Parameter(ParameterSetName = "File")]
        public Hashtable FieldMapping { get; set; }

        /// <summary>
        /// The batch size for import operations
        /// </summary>
        [Parameter()]
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Whether to continue on error
        /// </summary>
        [Parameter()]
        public SwitchParameter ContinueOnError { get; set; }

        /// <summary>
        /// Whether to update existing records
        /// </summary>
        [Parameter()]
        public SwitchParameter Update { get; set; }

        /// <summary>
        /// The field to use as the key for updates
        /// </summary>
        [Parameter()]
        public string KeyField { get; set; }

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
        /// The list of records to import
        /// </summary>
        private List<TeableRecord> _records = new List<TeableRecord>();

        /// <summary>
        /// Processes each record from the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Records" && Records != null)
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
                // Load records from file if specified
                if (ParameterSetName == "File")
                {
                    // Check if the file exists
                    if (!File.Exists(Path))
                    {
                        WriteError(new ErrorRecord(
                            new FileNotFoundException($"Data file not found: {Path}"),
                            "DataFileNotFound",
                            ErrorCategory.ObjectNotFound,
                            Path));
                        return;
                    }

                    // Load the records from the file
                    switch (Format)
                    {
                        case TeableImportFormat.Json:
                            _records = LoadFromJson(Path);
                            break;

                        case TeableImportFormat.Csv:
                            _records = LoadFromCsv(Path);
                            break;

                        default:
                            WriteError(new ErrorRecord(
                                new ArgumentException($"Unsupported import format: {Format}"),
                                "UnsupportedFormat",
                                ErrorCategory.InvalidArgument,
                                Format));
                            return;
                    }
                }

                // Check if we have any records to import
                if (_records.Count == 0)
                {
                    WriteWarning("No records to import");
                    return;
                }

                // Import the records
                if (Update && !string.IsNullOrEmpty(KeyField))
                {
                    // Update existing records
                    var result = UpdateRecords(_records);
                    WriteObject(result, true);
                }
                else
                {
                    // Create new records
                    var result = CreateRecords(_records);
                    WriteObject(result, true);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ImportDataFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }

        /// <summary>
        /// Loads records from a JSON file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The records</returns>
        private List<TeableRecord> LoadFromJson(string path)
        {
            // Read the file
            string json = File.ReadAllText(path);

            try
            {
                // Try to parse the JSON as an array of records
                var records = JsonSerializer.Deserialize<TeableRecord[]>(json);
                return new List<TeableRecord>(records);
            }
            catch (JsonException)
            {
                try
                {
                    // Try to parse the JSON as a single record
                    var record = JsonSerializer.Deserialize<TeableRecord>(json);
                    return new List<TeableRecord> { record };
                }
                catch (JsonException ex)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Invalid JSON: {ex.Message}"),
                        "InvalidJson",
                        ErrorCategory.InvalidData,
                        path));
                    return new List<TeableRecord>();
                }
            }
        }

        /// <summary>
        /// Loads records from a CSV file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The records</returns>
        private List<TeableRecord> LoadFromCsv(string path)
        {
            try
            {
                // Read the CSV file
                var (headers, rows) = CsvHelper.ReadCsvFile(path, !NoHeader, Delimiter);

                // Check if we have any data
                if (headers.Length == 0 || rows.Count == 0)
                {
                    WriteWarning("CSV file is empty or contains only a header row");
                    return new List<TeableRecord>();
                }

                // Process headers
                headers = ProcessHeaders(headers);

                // Convert rows to records
                return ConvertRowsToRecords(headers, rows);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "CsvParsingError",
                    ErrorCategory.InvalidData,
                    path));
                return new List<TeableRecord>();
            }
        }

        /// <summary>
        /// Processes the headers from a CSV file
        /// </summary>
        /// <param name="headers">The original headers</param>
        /// <returns>The processed headers</returns>
        private string[] ProcessHeaders(string[] headers)
        {
            // If no headers were provided, generate them
            if (headers.Length == 0 && NoHeader)
            {
                return GenerateHeaders();
            }

            // Apply field mapping if specified
            if (FieldMapping != null)
            {
                return ApplyFieldMapping(headers);
            }

            return headers;
        }

        /// <summary>
        /// Generates headers when none are provided
        /// </summary>
        /// <returns>The generated headers</returns>
        private string[] GenerateHeaders()
        {
            // Use field mapping or generate headers
            if (FieldMapping != null)
            {
                return FieldMapping.Keys
                    .Select(key => FieldMapping[key].ToString())
                    .ToArray();
            }
            else
            {
                // Generate default headers
                return Enumerable.Range(1, 10)
                    .Select(i => $"Field{i}")
                    .ToArray();
            }
        }

        /// <summary>
        /// Applies field mapping to headers
        /// </summary>
        /// <param name="headers">The original headers</param>
        /// <returns>The mapped headers</returns>
        private string[] ApplyFieldMapping(string[] headers)
        {
            var mappedHeaders = new string[headers.Length];
            Array.Copy(headers, mappedHeaders, headers.Length);

            for (int i = 0; i < mappedHeaders.Length; i++)
            {
                if (FieldMapping.ContainsKey(mappedHeaders[i]))
                {
                    mappedHeaders[i] = FieldMapping[mappedHeaders[i]].ToString();
                }
            }

            return mappedHeaders;
        }

        /// <summary>
        /// Converts CSV rows to TeableRecord objects
        /// </summary>
        /// <param name="headers">The headers</param>
        /// <param name="rows">The rows</param>
        /// <returns>The records</returns>
        private List<TeableRecord> ConvertRowsToRecords(string[] headers, List<string[]> rows)
        {
            var records = new List<TeableRecord>();

            foreach (var values in rows)
            {
                // Create a record
                var record = new TeableRecord
                {
                    Fields = new Dictionary<string, object>()
                };

                // Add the fields
                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    if (!string.IsNullOrEmpty(values[j]))
                    {
                        record.Fields[headers[j]] = values[j];
                    }
                }

                // Add the record
                records.Add(record);
            }

            return records;
        }

        /// <summary>
        /// Creates new records
        /// </summary>
        /// <param name="records">The records to create</param>
        /// <returns>The created records</returns>
        private List<TeableRecord> CreateRecords(List<TeableRecord> records)
        {
            var createdRecords = new List<TeableRecord>();

            // Process the records in batches
            for (int i = 0; i < records.Count; i += BatchSize)
            {
                // Get the current batch
                var batch = records.GetRange(i, Math.Min(BatchSize, records.Count - i));

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
                    new Uri(TeableUrlBuilder.GetRecordsUrl(TableId)))
                {
                    Content = content
                };

                try
                {
                    // Send the request
                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    // Check the response
                    if (response?.Data != null)
                    {
                        createdRecords.AddRange(response.Data);
                        WriteVerbose($"Created {response.Data.Count} records (batch {i / BatchSize + 1})");
                    }
                    else
                    {
                        WriteWarning($"Failed to create records in batch {i / BatchSize + 1}");

                        if (!ContinueOnError)
                        {
                            throw new InvalidOperationException("Failed to create records");
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteWarning($"Error creating records in batch {i / BatchSize + 1}: {ex.Message}");

                    if (!ContinueOnError)
                    {
                        throw;
                    }
                }
            }

            return createdRecords;
        }

        /// <summary>
        /// Updates existing records
        /// </summary>
        /// <param name="records">The records to update</param>
        /// <returns>The updated records</returns>
        private List<TeableRecord> UpdateRecords(List<TeableRecord> records)
        {
            // Check if we have a key field
            if (string.IsNullOrEmpty(KeyField))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException("KeyField is required for updates"),
                    "MissingKeyField",
                    ErrorCategory.InvalidArgument,
                    null));
                return new List<TeableRecord>();
            }

            // Get the existing records and create a lookup
            var recordLookup = CreateRecordLookup();

            // Separate records into updates and creates
            var (recordsToUpdate, recordsToCreate) = SeparateRecordsForUpdate(records, recordLookup);

            // Update existing records
            var updatedRecords = UpdateExistingRecords(recordsToUpdate);

            // Create new records
            if (recordsToCreate.Count > 0)
            {
                var createdRecords = CreateRecords(recordsToCreate);
                updatedRecords.AddRange(createdRecords);
            }

            return updatedRecords;
        }

        /// <summary>
        /// Creates a lookup of existing records by key field
        /// </summary>
        /// <returns>The record lookup</returns>
        private Dictionary<string, TeableRecord> CreateRecordLookup()
        {
            var existingRecords = GetExistingRecords();
            var recordLookup = new Dictionary<string, TeableRecord>();

            foreach (var record in existingRecords)
            {
                if (record.Fields != null && record.Fields.TryGetValue(KeyField, out var keyValue) && keyValue != null)
                {
                    recordLookup[keyValue.ToString()] = record;
                }
            }

            return recordLookup;
        }

        /// <summary>
        /// Separates records into those to update and those to create
        /// </summary>
        /// <param name="records">The records to process</param>
        /// <param name="recordLookup">The lookup of existing records</param>
        /// <returns>A tuple of records to update and records to create</returns>
        private (List<TeableRecord> ToUpdate, List<TeableRecord> ToCreate) SeparateRecordsForUpdate(
            List<TeableRecord> records,
            Dictionary<string, TeableRecord> recordLookup)
        {
            var recordsToUpdate = new List<TeableRecord>();
            var recordsToCreate = new List<TeableRecord>();

            foreach (var record in records)
            {
                if (record.Fields == null || !record.Fields.TryGetValue(KeyField, out var keyValue) || keyValue == null)
                {
                    WriteWarning($"Record is missing key field '{KeyField}'");
                    continue;
                }

                string key = keyValue.ToString();

                if (recordLookup.TryGetValue(key, out var existingRecord))
                {
                    // Mark for update
                    record.Id = existingRecord.Id;
                    recordsToUpdate.Add(record);
                }
                else
                {
                    // Mark for creation
                    recordsToCreate.Add(record);
                }
            }

            return (recordsToUpdate, recordsToCreate);
        }

        /// <summary>
        /// Updates existing records
        /// </summary>
        /// <param name="recordsToUpdate">The records to update</param>
        /// <returns>The updated records</returns>
        private List<TeableRecord> UpdateExistingRecords(List<TeableRecord> recordsToUpdate)
        {
            var updatedRecords = new List<TeableRecord>();

            foreach (var record in recordsToUpdate)
            {
                try
                {
                    var updatedRecord = UpdateRecord(record);
                    if (updatedRecord != null)
                    {
                        updatedRecords.Add(updatedRecord);
                    }
                }
                catch (Exception ex)
                {
                    string key = record.Fields[KeyField].ToString();
                    WriteWarning($"Error updating record with key '{key}': {ex.Message}");

                    if (!ContinueOnError)
                    {
                        throw;
                    }
                }
            }

            return updatedRecords;
        }

        /// <summary>
        /// Updates a single record
        /// </summary>
        /// <param name="record">The record to update</param>
        /// <returns>The updated record</returns>
        private TeableRecord UpdateRecord(TeableRecord record)
        {
            // Create the request body
            var requestBody = new
            {
                fields = record.Fields
            };

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
            var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableRecord>>(
                request,
                this,
                RespectRateLimit,
                RateLimitDelay);

            // Check the response
            if (response?.Data != null)
            {
                WriteVerbose($"Updated record {record.Id}");
                return response.Data;
            }
            else
            {
                WriteWarning($"Failed to update record {record.Id}");
                return null;
            }
        }

        /// <summary>
        /// Gets existing records from the table
        /// </summary>
        /// <returns>The existing records</returns>
        private List<TeableRecord> GetExistingRecords()
        {
            var records = new List<TeableRecord>();
            string pageToken = null;

            do
            {
                // Build the URL
                string url = TeableUrlBuilder.GetRecordsUrl(
                    TableId,
                    null,
                    null,
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

                    WriteVerbose($"Retrieved {response.Data.Count} existing records (total: {records.Count})");
                }
                else
                {
                    pageToken = null;
                }
            }
            while (!string.IsNullOrEmpty(pageToken));

            return records;
        }
    }
}








