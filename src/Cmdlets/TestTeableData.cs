using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Tests Teable data for validity
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "TeableData")]
    [OutputType(typeof(PSObject))]
    public class TestTeableData : PSCmdlet
    {
        /// <summary>
        /// The path to the data file
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "File")]
        public string Path { get; set; }
        
        /// <summary>
        /// The records to test
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Records")]
        public TeableRecord[] Records { get; set; }
        
        /// <summary>
        /// The ID of the table to validate against
        /// </summary>
        [Parameter(Mandatory = true)]
        public string TableId { get; set; }
        
        /// <summary>
        /// Whether to return detailed validation results
        /// </summary>
        [Parameter()]
        public SwitchParameter Detailed { get; set; }
        
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
        /// The list of records to validate
        /// </summary>
        private List<TeableRecord> _records = new List<TeableRecord>();
        
        /// <summary>
        /// The fields for the table
        /// </summary>
        private List<TeableField> _fields;
        
        /// <summary>
        /// Initializes the cmdlet
        /// </summary>
        protected override void BeginProcessing()
        {
            try
            {
                // Get the fields for the table
                var request = new System.Net.Http.HttpRequestMessage(
                    System.Net.Http.HttpMethod.Get,
                    new Uri(TeableUrlBuilder.GetFieldsUrl(TableId)));
                
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (response?.Data == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Failed to get fields for table {TableId}"),
                        "GetFieldsFailed",
                        ErrorCategory.InvalidOperation,
                        TableId));
                }
                
                _fields = response.Data;
                
                // If using a file, load the records
                if (ParameterSetName == "File")
                {
                    // Check if the file exists
                    if (!File.Exists(Path))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"Data file not found: {Path}"),
                            "DataFileNotFound",
                            ErrorCategory.ObjectNotFound,
                            Path));
                    }
                    
                    // Read the data file
                    string json = File.ReadAllText(Path);
                    
                    try
                    {
                        // Try to parse the JSON as an array of records
                        var records = JsonSerializer.Deserialize<TeableRecord[]>(json);
                        _records.AddRange(records);
                    }
                    catch (JsonException)
                    {
                        try
                        {
                            // Try to parse the JSON as a single record
                            var record = JsonSerializer.Deserialize<TeableRecord>(json);
                            _records.Add(record);
                        }
                        catch (JsonException ex)
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new Exception($"Invalid JSON: {ex.Message}"),
                                "InvalidJson",
                                ErrorCategory.InvalidData,
                                Path));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "TestDataFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
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
                // Validate the records
                var validationResults = ValidateRecords(_records);
                
                if (validationResults.Count == 0)
                {
                    if (Detailed)
                    {
                        WriteObject(new PSObject
                        {
                            Properties =
                            {
                                new PSNoteProperty("Valid", true),
                                new PSNoteProperty("RecordCount", _records.Count)
                            }
                        });
                    }
                    else
                    {
                        WriteObject(true);
                    }
                }
                else
                {
                    if (Detailed)
                    {
                        WriteObject(new PSObject
                        {
                            Properties =
                            {
                                new PSNoteProperty("Valid", false),
                                new PSNoteProperty("RecordCount", _records.Count),
                                new PSNoteProperty("ErrorCount", validationResults.Count),
                                new PSNoteProperty("Errors", validationResults)
                            }
                        });
                    }
                    else
                    {
                        WriteObject(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "TestDataFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Validates a list of records
        /// </summary>
        /// <param name="records">The records to validate</param>
        /// <returns>A list of validation errors</returns>
        private List<string> ValidateRecords(List<TeableRecord> records)
        {
            var errors = new List<string>();
            
            // Create a lookup of fields by ID
            var fieldLookup = new Dictionary<string, TeableField>();
            foreach (var field in _fields)
            {
                fieldLookup[field.Id] = field;
            }
            
            // Validate each record
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                
                // Check if the record has fields
                if (record.Fields == null)
                {
                    errors.Add($"Record {i}: Missing fields");
                    continue;
                }
                
                // Validate each field
                foreach (var field in record.Fields)
                {
                    // Check if the field exists in the table
                    if (!fieldLookup.TryGetValue(field.Key, out var fieldInfo))
                    {
                        errors.Add($"Record {i}: Field '{field.Key}' does not exist in the table");
                        continue;
                    }
                    
                    // Validate the field value
                    string error = ValidateFieldValue(field.Value, fieldInfo, i);
                    if (!string.IsNullOrEmpty(error))
                    {
                        errors.Add(error);
                    }
                }
            }
            
            return errors;
        }
        
        /// <summary>
        /// Validates a field value
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="field">The field information</param>
        /// <param name="recordIndex">The index of the record</param>
        /// <returns>An error message, or null if the value is valid</returns>
        private string ValidateFieldValue(object value, TeableField field, int recordIndex)
        {
            if (value == null)
            {
                // Check if the field is required
                if (field.Options?.Required == true)
                {
                    return $"Record {recordIndex}: Field '{field.Name}' is required";
                }
                
                return null;
            }
            
            // Validate based on field type
            switch (field.Type)
            {
                case "singleLineText":
                case "longText":
                    if (!(value is string))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be a string";
                    }
                    break;
                
                case "number":
                case "currency":
                case "percent":
                    if (!(value is double || value is int || value is long || value is decimal))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be a number";
                    }
                    break;
                
                case "checkbox":
                    if (!(value is bool))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be a boolean";
                    }
                    break;
                
                case "date":
                case "dateTime":
                    if (!(value is DateTime || value is string))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be a date";
                    }
                    break;
                
                case "singleSelect":
                    if (!(value is string))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be a string";
                    }
                    
                    // Check if the value is in the options
                    if (field.Options != null && field.Options.TryGetValue("choices", out var choices))
                    {
                        if (choices is JsonElement choicesElement && choicesElement.ValueKind == JsonValueKind.Array)
                        {
                            bool found = false;
                            foreach (var choice in choicesElement.EnumerateArray())
                            {
                                if (choice.TryGetProperty("name", out var nameElement) && 
                                    nameElement.GetString() == value.ToString())
                                {
                                    found = true;
                                    break;
                                }
                            }
                            
                            if (!found)
                            {
                                return $"Record {recordIndex}: Field '{field.Name}' has an invalid value '{value}'";
                            }
                        }
                    }
                    break;
                
                case "multipleSelect":
                    if (!(value is string[] || value is List<string>))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be an array of strings";
                    }
                    break;
                
                case "attachment":
                    if (!(value is Dictionary<string, object>[] || value is List<Dictionary<string, object>>))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be an array of objects";
                    }
                    break;
                
                case "link":
                    if (!(value is string[] || value is List<string>))
                    {
                        return $"Record {recordIndex}: Field '{field.Name}' must be an array of strings";
                    }
                    break;
            }
            
            return null;
        }
    }
}

