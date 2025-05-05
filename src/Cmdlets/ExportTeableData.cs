using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Export format for Teable data
    /// </summary>
    public enum TeableExportFormat
    {
        /// <summary>
        /// JSON format
        /// </summary>
        Json,

        /// <summary>
        /// XML format
        /// </summary>
        Xml,

        /// <summary>
        /// CSV format
        /// </summary>
        Csv
    }

    /// <summary>
    /// Exports data from a Teable table or view
    /// </summary>
    [Cmdlet(VerbsData.Export, "TeableData")]
    [Alias("Export-TeableRecords")]
    [OutputType(typeof(TeableRecord[]))]
    public class ExportTeableData : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to export data from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Table")]
        public string TableId { get; set; }

        /// <summary>
        /// The ID of the view to export data from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "View")]
        public string ViewId { get; set; }

        /// <summary>
        /// The records to export (from pipeline)
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Pipeline")]
        public TeableRecord[] Records { get; set; }

        /// <summary>
        /// The path to save the data to
        /// </summary>
        [Parameter(Position = 1)]
        public string Path { get; set; }

        /// <summary>
        /// The format to export the data in
        /// </summary>
        [Parameter()]
        public TeableExportFormat Format { get; set; } = TeableExportFormat.Json;

        /// <summary>
        /// The filter to apply to the data
        /// </summary>
        [Parameter(ParameterSetName = "Table")]
        [Parameter(ParameterSetName = "View")]
        public TeableFilter Filter { get; set; }

        /// <summary>
        /// The sort to apply to the data
        /// </summary>
        [Parameter(ParameterSetName = "Table")]
        [Parameter(ParameterSetName = "View")]
        public TeableSort Sort { get; set; }

        /// <summary>
        /// The fields to include in the export
        /// </summary>
        [Parameter()]
        public string[] Fields { get; set; }

        /// <summary>
        /// Whether to flatten nested objects in CSV export
        /// </summary>
        [Parameter()]
        public SwitchParameter Flatten { get; set; }

        /// <summary>
        /// The delimiter to use for CSV export
        /// </summary>
        [Parameter()]
        public string Delimiter { get; set; } = ",";

        /// <summary>
        /// Whether to include headers in CSV export
        /// </summary>
        [Parameter()]
        public SwitchParameter NoHeader { get; set; }

        /// <summary>
        /// Whether to respect rate limits
        /// </summary>
        [Parameter(ParameterSetName = "Table")]
        [Parameter(ParameterSetName = "View")]
        public SwitchParameter RespectRateLimit { get; set; }

        /// <summary>
        /// The delay to use when rate limited
        /// </summary>
        [Parameter(ParameterSetName = "Table")]
        [Parameter(ParameterSetName = "View")]
        public TimeSpan RateLimitDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The list of records to export (collected from pipeline)
        /// </summary>
        private List<TeableRecord> _records = new List<TeableRecord>();

        /// <summary>
        /// The fields metadata for the table or view
        /// </summary>
        private List<TeableField> _fieldsMetadata = null;

        /// <summary>
        /// Initializes the cmdlet
        /// </summary>
        protected override void BeginProcessing()
        {
            if (ParameterSetName == "Table" || ParameterSetName == "View")
            {
                // Get the fields metadata
                try
                {
                    string tableId = TableId;

                    // If we're exporting from a view, get the table ID from the view
                    if (ParameterSetName == "View")
                    {
                        var viewRequest = new HttpRequestMessage(
                            HttpMethod.Get,
                            new Uri(TeableUrlBuilder.GetViewUrl(ViewId)));

                        var viewResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableView>>(
                            viewRequest,
                            this,
                            RespectRateLimit,
                            RateLimitDelay);

                        if (viewResponse?.Data == null)
                        {
                            WriteError(new ErrorRecord(
                                new Exception($"View {ViewId} not found"),
                                "ViewNotFound",
                                ErrorCategory.ObjectNotFound,
                                null));
                            return;
                        }

                        tableId = viewResponse.Data.TableId;
                    }

                    // Get the fields metadata
                    var fieldsRequest = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetFieldsUrl(tableId)));

                    var fieldsResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                        fieldsRequest,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    if (fieldsResponse?.Data != null)
                    {
                        _fieldsMetadata = fieldsResponse.Data;
                    }
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to get fields metadata: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Processes each record from the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Pipeline" && Records != null)
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
                // Get records from the API if not from pipeline
                if (ParameterSetName == "Table" || ParameterSetName == "View")
                {
                    _records = GetRecordsFromApi();
                }

                // Export the records
                if (_records.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Path))
                    {
                        // Ensure the directory exists
                        string directory = System.IO.Path.GetDirectoryName(Path);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Export to a file
                        switch (Format)
                        {
                            case TeableExportFormat.Json:
                                ExportToJson(_records, Path);
                                break;

                            case TeableExportFormat.Xml:
                                ExportToXml(_records, Path);
                                break;

                            case TeableExportFormat.Csv:
                                ExportToCsv(_records, Path);
                                break;

                            default:
                                throw new ArgumentException($"Unsupported export format: {Format}");
                        }

                        Logger.Verbose(this, $"Exported {_records.Count} records to {Path}");
                    }

                    // Return the records
                    WriteObject(_records, true);
                }
                else
                {
                    WriteWarning("No records to export");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ExportDataFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }

        /// <summary>
        /// Gets records from the API
        /// </summary>
        /// <returns>The records</returns>
        private List<TeableRecord> GetRecordsFromApi()
        {
            var records = new List<TeableRecord>();
            string pageToken = null;

            do
            {
                // Build the URL
                string url;
                if (ParameterSetName == "Table")
                {
                    url = TeableUrlBuilder.GetRecordsUrl(
                        TableId,
                        null,
                        Filter?.ToQueryString(),
                        Sort?.ToQueryString(),
                        Fields != null ? string.Join(",", Fields) : null,
                        100, // Page size
                        pageToken);
                }
                else // View
                {
                    url = TeableUrlBuilder.GetViewRecordsUrl(
                        ViewId,
                        Filter?.ToQueryString(),
                        Sort?.ToQueryString(),
                        Fields != null ? string.Join(",", Fields) : null,
                        100, // Page size
                        pageToken);
                }

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

                    Logger.Verbose(this, $"Retrieved {response.Data.Count} records (total: {records.Count})");
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
        /// Exports records to a JSON file
        /// </summary>
        /// <param name="records">The records to export</param>
        /// <param name="path">The path to save the file to</param>
        private void ExportToJson(List<TeableRecord> records, string path)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(records, options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Exports records to an XML file
        /// </summary>
        /// <param name="records">The records to export</param>
        /// <param name="path">The path to save the file to</param>
        private void ExportToXml(List<TeableRecord> records, string path)
        {
            // Create a wrapper object for the records
            var wrapper = new
            {
                Records = records
            };

            // Create the XML serializer
            var serializer = new XmlSerializer(wrapper.GetType());

            // Create the XML writer settings
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };

            // Write the XML file
            using var writer = XmlWriter.Create(path, settings);
            serializer.Serialize(writer, wrapper);
        }

        /// <summary>
        /// Exports records to a CSV file
        /// </summary>
        /// <param name="records">The records to export</param>
        /// <param name="path">The path to save the file to</param>
        private void ExportToCsv(List<TeableRecord> records, string path)
        {
            // Get all field names
            var fieldNames = new HashSet<string>();

            // If we have fields metadata, use it to get the field names in the correct order
            if (_fieldsMetadata != null && _fieldsMetadata.Count > 0)
            {
                foreach (var field in _fieldsMetadata)
                {
                    fieldNames.Add(field.Name);
                }
            }

            // Add any additional fields from the records
            foreach (var record in records)
            {
                if (record.Fields != null)
                {
                    // Get field names from the dictionary
                    foreach (var property in record.Fields)
                    {
                        fieldNames.Add(property.Key);
                    }
                }
            }

            // Filter fields if specified
            if (Fields != null && Fields.Length > 0)
            {
                fieldNames = new HashSet<string>(fieldNames.Intersect(Fields));
            }

            // Create the CSV header
            var header = new List<string> { "Id" };
            header.AddRange(fieldNames);

            // Create the CSV rows
            var rows = new List<string[]>();

            foreach (var record in records)
            {
                var row = new string[header.Count];
                row[0] = record.Id;

                int index = 1;
                foreach (var fieldName in fieldNames)
                {
                    string value = "";

                    if (record.Fields != null && record.Fields.TryGetValue(fieldName, out var fieldObj))
                    {
                        // Convert the object to a string based on its type
                        if (fieldObj is Dictionary<string, object> dictObj && Flatten)
                        {
                            // Flatten dictionary objects
                            value = string.Join("; ", dictObj.Select(kv => $"{kv.Key}:{FlattenValue(kv.Value)}"));
                        }
                        else if (fieldObj is List<object> listObj && Flatten)
                        {
                            // Flatten list objects
                            value = string.Join("; ", listObj.Select(FlattenValue));
                        }
                        else
                        {
                            // Use the raw value
                            value = FlattenValue(fieldObj);
                        }
                    }

                    row[index++] = value;
                }

                rows.Add(row);
            }

            // Write the CSV file
            CsvHelper.WriteCsvFile(
                path,
                NoHeader ? new string[0] : header.ToArray(),
                rows,
                Delimiter);
        }

        /// <summary>
        /// Flattens a value to a string
        /// </summary>
        /// <param name="value">The value to flatten</param>
        /// <returns>The flattened value</returns>
        private string FlattenValue(object value)
        {
            if (value == null)
            {
                return "";
            }

            if (value is string || value is int || value is long || value is double || value is decimal || value is bool)
            {
                return value.ToString();
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString("o");
            }

            if (value is IEnumerable<object> enumerable)
            {
                var values = new List<string>();

                foreach (var item in enumerable)
                {
                    values.Add(FlattenValue(item));
                }

                return string.Join(";", values);
            }

            if (value is IDictionary<string, object> dictionary)
            {
                var values = new List<string>();

                foreach (var kvp in dictionary)
                {
                    values.Add($"{kvp.Key}:{FlattenValue(kvp.Value)}");
                }

                return string.Join(";", values);
            }

            // Serialize the value to JSON
            return JsonSerializer.Serialize(value);
        }
    }
}



