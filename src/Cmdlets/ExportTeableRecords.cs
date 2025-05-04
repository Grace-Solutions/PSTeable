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
    /// Export format options for Teable records
    /// </summary>
    public enum TeableExportFormat
    {
        /// <summary>
        /// Export as JSON
        /// </summary>
        Json,

        /// <summary>
        /// Export as XML
        /// </summary>
        Xml,

        /// <summary>
        /// Export as CSV
        /// </summary>
        Csv
    }

    /// <summary>
    /// Exports records from a Teable table or view
    /// </summary>
    [Cmdlet(VerbsData.Export, "TeableRecords")]
    [OutputType(typeof(void))]
    public class ExportTeableRecords : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to export records from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Table")]
        public string TableId { get; set; }

        /// <summary>
        /// The ID of the view to export records from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "View")]
        public string ViewId { get; set; }

        /// <summary>
        /// The records to export (from pipeline)
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Pipeline")]
        public TeableRecord[] Records { get; set; }

        /// <summary>
        /// The path to save the records to
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; }

        /// <summary>
        /// The format to export the records in
        /// </summary>
        [Parameter()]
        public TeableExportFormat Format { get; set; } = TeableExportFormat.Json;

        /// <summary>
        /// Whether to flatten nested objects when exporting to CSV
        /// </summary>
        [Parameter()]
        public SwitchParameter FlattenObjects { get; set; }

        /// <summary>
        /// The filter to apply to the records
        /// </summary>
        [Parameter(ParameterSetName = "Table")]
        [Parameter(ParameterSetName = "View")]
        public string Filter { get; set; }

        /// <summary>
        /// The fields to include in the export
        /// </summary>
        [Parameter()]
        public string[] Fields { get; set; }

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
                if (ParameterSetName == "Table" || ParameterSetName == "View")
                {
                    // Get records from the API
                    GetRecordsFromApi();
                }

                // Export the records
                if (_records.Count > 0)
                {
                    switch (Format)
                    {
                        case TeableExportFormat.Json:
                            ExportToJson();
                            break;
                        case TeableExportFormat.Xml:
                            ExportToXml();
                            break;
                        case TeableExportFormat.Csv:
                            ExportToCsv();
                            break;
                    }

                    WriteVerbose($"Exported {_records.Count} records to {Path}");
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
                    "ExportRecordsFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }

        /// <summary>
        /// Gets records from the API
        /// </summary>
        private void GetRecordsFromApi()
        {
            // Build the fields string
            string fieldsString = null;
            if (Fields != null && Fields.Length > 0)
            {
                fieldsString = string.Join(",", Fields);
            }

            // Get records with pagination
            string pageToken = null;
            string url;

            do
            {
                if (ParameterSetName == "Table")
                {
                    url = TeableUrlBuilder.GetRecordsUrl(
                        TableId,
                        null,
                        Filter,
                        null,
                        fieldsString,
                        100, // Page size
                        pageToken);
                }
                else // View
                {
                    url = TeableUrlBuilder.GetViewRecordsUrl(
                        ViewId,
                        Filter,
                        null,
                        fieldsString,
                        100, // Page size
                        pageToken);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response?.Data != null)
                {
                    _records.AddRange(response.Data);
                    pageToken = response.NextPageToken;
                }
                else
                {
                    pageToken = null;
                }
            }
            while (!string.IsNullOrEmpty(pageToken));
        }

        /// <summary>
        /// Exports records to JSON
        /// </summary>
        private void ExportToJson()
        {
            var json = JsonSerializer.Serialize(_records, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(Path, json);
        }

        /// <summary>
        /// Exports records to XML
        /// </summary>
        private void ExportToXml()
        {
            var serializer = new XmlSerializer(typeof(List<TeableRecord>));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };

            using var writer = XmlWriter.Create(Path, settings);
            serializer.Serialize(writer, _records);
        }

        /// <summary>
        /// Exports records to CSV
        /// </summary>
        private void ExportToCsv()
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
            foreach (var record in _records)
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

            // Create the CSV header
            var header = new List<string> { "Id" };
            header.AddRange(fieldNames);

            // Create the CSV rows
            var rows = new List<string>();
            rows.Add(string.Join(",", header.Select(EscapeCsvField)));

            foreach (var record in _records)
            {
                var row = new List<string> { EscapeCsvField(record.Id) };

                foreach (var fieldName in fieldNames)
                {
                    string value = "";

                    if (record.Fields != null)
                    {
                        // Get the field value from the dictionary
                        if (record.Fields.TryGetValue(fieldName, out var fieldObj))
                        {
                            // Convert the object to a string based on its type
                            if (fieldObj is Dictionary<string, object> dictObj && FlattenObjects)
                            {
                                // Flatten dictionary objects
                                value = string.Join("; ", dictObj.Select(kv => $"{kv.Key}:{kv.Value}"));
                            }
                            else if (fieldObj is List<object> listObj && FlattenObjects)
                            {
                                // Flatten list objects
                                value = string.Join("; ", listObj);
                            }
                            else
                            {
                                // Use the raw value
                                value = fieldObj?.ToString() ?? "";
                            }
                        }
                    }

                    row.Add(EscapeCsvField(value));
                }

                rows.Add(string.Join(",", row));
            }

            // Write the CSV file
            File.WriteAllLines(Path, rows);
        }

        /// <summary>
        /// Escapes a field for CSV output
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return "";
            }

            // If the field contains a comma, newline, or double quote, wrap it in quotes
            if (field.Contains(",") || field.Contains("\n") || field.Contains("\""))
            {
                // Replace double quotes with two double quotes
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }


    }
}
