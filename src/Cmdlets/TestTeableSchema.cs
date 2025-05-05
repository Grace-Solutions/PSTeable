using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text.Json;
using PSTeable.Models;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Tests a Teable schema for validity
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "TeableSchema")]
    [OutputType(typeof(PSObject))]
    public class TestTeableSchema : PSCmdlet
    {
        /// <summary>
        /// The path to the schema file
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        
        /// <summary>
        /// Whether to return detailed validation results
        /// </summary>
        [Parameter()]
        public SwitchParameter Detailed { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Check if the file exists
                if (!File.Exists(Path))
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"Schema file not found: {Path}"),
                        "SchemaFileNotFound",
                        ErrorCategory.ObjectNotFound,
                        Path));
                    return;
                }
                
                // Read the schema file
                string json = File.ReadAllText(Path);
                
                // Try to parse the JSON
                JsonDocument schemaDoc;
                try
                {
                    schemaDoc = JsonDocument.Parse(json);
                }
                catch (JsonException ex)
                {
                    if (Detailed)
                    {
                        WriteObject(new PSObject
                        {
                            Properties =
                            {
                                new PSNoteProperty("Valid", false),
                                new PSNoteProperty("Error", $"Invalid JSON: {ex.Message}"),
                                new PSNoteProperty("Line", ex.LineNumber),
                                new PSNoteProperty("Position", ex.BytePositionInLine)
                            }
                        });
                    }
                    else
                    {
                        WriteObject(false);
                    }
                    return;
                }
                
                // Validate the schema
                var validationResults = ValidateSchema(schemaDoc.RootElement);
                
                if (validationResults.Count == 0)
                {
                    if (Detailed)
                    {
                        WriteObject(new PSObject
                        {
                            Properties =
                            {
                                new PSNoteProperty("Valid", true),
                                new PSNoteProperty("Type", schemaDoc.RootElement.TryGetProperty("Type", out var typeElement) ? typeElement.GetString() : "Unknown")
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
                    "TestSchemaFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Validates a schema
        /// </summary>
        /// <param name="schema">The schema to validate</param>
        /// <returns>A list of validation errors</returns>
        private List<string> ValidateSchema(JsonElement schema)
        {
            var errors = new List<string>();
            
            // Check if the schema has a Type property
            if (!schema.TryGetProperty("Type", out var typeElement))
            {
                errors.Add("Missing Type property");
                return errors;
            }
            
            // Check the schema type
            string schemaType = typeElement.GetString();
            if (schemaType == "Base")
            {
                ValidateBaseSchema(schema, errors);
            }
            else if (schemaType == "View")
            {
                ValidateViewSchema(schema, errors);
            }
            else
            {
                errors.Add($"Invalid schema type: {schemaType}");
            }
            
            return errors;
        }
        
        /// <summary>
        /// Validates a base schema
        /// </summary>
        /// <param name="schema">The schema to validate</param>
        /// <param name="errors">The list of errors to add to</param>
        private void ValidateBaseSchema(JsonElement schema, List<string> errors)
        {
            // Check required properties
            if (!schema.TryGetProperty("Id", out _))
            {
                errors.Add("Missing Id property");
            }
            
            if (!schema.TryGetProperty("Name", out _))
            {
                errors.Add("Missing Name property");
            }
            
            if (!schema.TryGetProperty("SpaceId", out _))
            {
                errors.Add("Missing SpaceId property");
            }
            
            // Check Tables property
            if (!schema.TryGetProperty("Tables", out var tablesElement))
            {
                errors.Add("Missing Tables property");
            }
            else if (tablesElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("Tables property must be an array");
            }
            else
            {
                // Validate each table
                int tableIndex = 0;
                foreach (var tableElement in tablesElement.EnumerateArray())
                {
                    ValidateTableSchema(tableElement, errors, tableIndex++);
                }
            }
            
            // Check Views property if present
            if (schema.TryGetProperty("Views", out var viewsElement))
            {
                if (viewsElement.ValueKind != JsonValueKind.Array && viewsElement.ValueKind != JsonValueKind.Null)
                {
                    errors.Add("Views property must be an array or null");
                }
                else if (viewsElement.ValueKind == JsonValueKind.Array)
                {
                    // Validate each view
                    int viewIndex = 0;
                    foreach (var viewElement in viewsElement.EnumerateArray())
                    {
                        ValidateViewInBaseSchema(viewElement, errors, viewIndex++);
                    }
                }
            }
        }
        
        /// <summary>
        /// Validates a table schema
        /// </summary>
        /// <param name="schema">The schema to validate</param>
        /// <param name="errors">The list of errors to add to</param>
        /// <param name="index">The index of the table</param>
        private void ValidateTableSchema(JsonElement schema, List<string> errors, int index)
        {
            // Check required properties
            if (!schema.TryGetProperty("Id", out _))
            {
                errors.Add($"Table {index}: Missing Id property");
            }
            
            if (!schema.TryGetProperty("Name", out _))
            {
                errors.Add($"Table {index}: Missing Name property");
            }
            
            // Check Fields property
            if (!schema.TryGetProperty("Fields", out var fieldsElement))
            {
                errors.Add($"Table {index}: Missing Fields property");
            }
            else if (fieldsElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add($"Table {index}: Fields property must be an array");
            }
            else
            {
                // Validate each field
                int fieldIndex = 0;
                foreach (var fieldElement in fieldsElement.EnumerateArray())
                {
                    ValidateFieldSchema(fieldElement, errors, index, fieldIndex++);
                }
            }
        }
        
        /// <summary>
        /// Validates a field schema
        /// </summary>
        /// <param name="schema">The schema to validate</param>
        /// <param name="errors">The list of errors to add to</param>
        /// <param name="tableIndex">The index of the table</param>
        /// <param name="index">The index of the field</param>
        private void ValidateFieldSchema(JsonElement schema, List<string> errors, int tableIndex, int index)
        {
            // Check required properties
            if (!schema.TryGetProperty("id", out _))
            {
                errors.Add($"Table {tableIndex}, Field {index}: Missing id property");
            }
            
            if (!schema.TryGetProperty("name", out _))
            {
                errors.Add($"Table {tableIndex}, Field {index}: Missing name property");
            }
            
            if (!schema.TryGetProperty("type", out var typeElement))
            {
                errors.Add($"Table {tableIndex}, Field {index}: Missing type property");
            }
            else
            {
                // Check if the field type is valid
                string fieldType = typeElement.GetString();
                if (string.IsNullOrEmpty(fieldType))
                {
                    errors.Add($"Table {tableIndex}, Field {index}: Empty type property");
                }
                else if (!IsValidFieldType(fieldType))
                {
                    errors.Add($"Table {tableIndex}, Field {index}: Invalid field type: {fieldType}");
                }
            }
            
            // Check options property if present
            if (schema.TryGetProperty("options", out var optionsElement))
            {
                if (optionsElement.ValueKind != JsonValueKind.Object && optionsElement.ValueKind != JsonValueKind.Null)
                {
                    errors.Add($"Table {tableIndex}, Field {index}: options property must be an object or null");
                }
            }
        }
        
        /// <summary>
        /// Validates a view in a base schema
        /// </summary>
        /// <param name="schema">The schema to validate</param>
        /// <param name="errors">The list of errors to add to</param>
        /// <param name="index">The index of the view</param>
        private void ValidateViewInBaseSchema(JsonElement schema, List<string> errors, int index)
        {
            // Check required properties
            if (!schema.TryGetProperty("Id", out _))
            {
                errors.Add($"View {index}: Missing Id property");
            }
            
            if (!schema.TryGetProperty("Name", out _))
            {
                errors.Add($"View {index}: Missing Name property");
            }
            
            if (!schema.TryGetProperty("TableId", out _))
            {
                errors.Add($"View {index}: Missing TableId property");
            }
            
            if (!schema.TryGetProperty("Type", out var typeElement))
            {
                errors.Add($"View {index}: Missing Type property");
            }
            else
            {
                // Check if the view type is valid
                string viewType = typeElement.GetString();
                if (string.IsNullOrEmpty(viewType))
                {
                    errors.Add($"View {index}: Empty Type property");
                }
                else if (!IsValidViewType(viewType))
                {
                    errors.Add($"View {index}: Invalid view type: {viewType}");
                }
            }
        }
        
        /// <summary>
        /// Validates a view schema
        /// </summary>
        /// <param name="schema">The schema to validate</param>
        /// <param name="errors">The list of errors to add to</param>
        private void ValidateViewSchema(JsonElement schema, List<string> errors)
        {
            // Check required properties
            if (!schema.TryGetProperty("Id", out _))
            {
                errors.Add("Missing Id property");
            }
            
            if (!schema.TryGetProperty("Name", out _))
            {
                errors.Add("Missing Name property");
            }
            
            if (!schema.TryGetProperty("TableId", out _))
            {
                errors.Add("Missing TableId property");
            }
            
            if (!schema.TryGetProperty("TableName", out _))
            {
                errors.Add("Missing TableName property");
            }
            
            if (!schema.TryGetProperty("ViewType", out var typeElement))
            {
                errors.Add("Missing ViewType property");
            }
            else
            {
                // Check if the view type is valid
                string viewType = typeElement.GetString();
                if (string.IsNullOrEmpty(viewType))
                {
                    errors.Add("Empty ViewType property");
                }
                else if (!IsValidViewType(viewType))
                {
                    errors.Add($"Invalid view type: {viewType}");
                }
            }
            
            // Check Fields property
            if (!schema.TryGetProperty("Fields", out var fieldsElement))
            {
                errors.Add("Missing Fields property");
            }
            else if (fieldsElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("Fields property must be an array");
            }
            else
            {
                // Validate each field
                int fieldIndex = 0;
                foreach (var fieldElement in fieldsElement.EnumerateArray())
                {
                    ValidateFieldSchema(fieldElement, errors, 0, fieldIndex++);
                }
            }
        }
        
        /// <summary>
        /// Checks if a field type is valid
        /// </summary>
        /// <param name="fieldType">The field type to check</param>
        /// <returns>True if the field type is valid, false otherwise</returns>
        private bool IsValidFieldType(string fieldType)
        {
            // List of valid field types
            string[] validTypes = new[]
            {
                "singleLineText",
                "longText",
                "singleSelect",
                "multipleSelect",
                "number",
                "currency",
                "percent",
                "date",
                "dateTime",
                "checkbox",
                "attachment",
                "user",
                "link",
                "lookup",
                "formula",
                "rollup",
                "count",
                "autoNumber",
                "createdTime",
                "lastModifiedTime",
                "createdBy",
                "lastModifiedBy"
            };
            
            return Array.IndexOf(validTypes, fieldType) >= 0;
        }
        
        /// <summary>
        /// Checks if a view type is valid
        /// </summary>
        /// <param name="viewType">The view type to check</param>
        /// <returns>True if the view type is valid, false otherwise</returns>
        private bool IsValidViewType(string viewType)
        {
            // List of valid view types
            string[] validTypes = new[]
            {
                "grid",
                "kanban",
                "gallery",
                "calendar",
                "gantt",
                "form"
            };
            
            return Array.IndexOf(validTypes, viewType) >= 0;
        }
    }
}
