using System;
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
    /// Imports a schema to create or update a Teable base or view
    /// </summary>
    [Cmdlet(VerbsData.Import, "TeableSchema")]
    [OutputType(typeof(void))]
    public class ImportTeableSchema : PSCmdlet
    {
        /// <summary>
        /// The path to the schema file
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }

        /// <summary>
        /// The ID of the space to create the base in (only used when creating a new base)
        /// </summary>
        [Parameter()]
        public string SpaceId { get; set; }

        /// <summary>
        /// Whether to update an existing base or view instead of creating a new one
        /// </summary>
        [Parameter()]
        public SwitchParameter Update { get; set; }

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
                // Read the schema file
                if (!File.Exists(Path))
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"Schema file not found: {Path}"),
                        "SchemaFileNotFound",
                        ErrorCategory.ObjectNotFound,
                        null));
                    return;
                }

                var json = File.ReadAllText(Path);
                var schemaDoc = JsonDocument.Parse(json);
                var root = schemaDoc.RootElement;

                // Check the schema type
                if (!root.TryGetProperty("Type", out var typeElement))
                {
                    WriteError(new ErrorRecord(
                        new Exception("Invalid schema format: missing Type property"),
                        "InvalidSchemaFormat",
                        ErrorCategory.InvalidData,
                        null));
                    return;
                }

                var schemaType = typeElement.GetString();
                if (schemaType == "Base")
                {
                    ImportBaseSchema(root);
                }
                else if (schemaType == "View")
                {
                    ImportViewSchema(root);
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Unsupported schema type: {schemaType}"),
                        "UnsupportedSchemaType",
                        ErrorCategory.InvalidData,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ImportSchemaFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }

        private void ImportBaseSchema(JsonElement schema)
        {
            // Check if we're updating an existing base or creating a new one
            string baseId = null;
            if (schema.TryGetProperty("Id", out var idElement))
            {
                baseId = idElement.GetString();
            }

            if (Update && string.IsNullOrEmpty(baseId))
            {
                WriteError(new ErrorRecord(
                    new Exception("Cannot update base: missing Id in schema"),
                    "MissingBaseId",
                    ErrorCategory.InvalidData,
                    null));
                return;
            }

            if (!Update && string.IsNullOrEmpty(SpaceId))
            {
                WriteError(new ErrorRecord(
                    new Exception("SpaceId is required when creating a new base"),
                    "MissingSpaceId",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Get the base name
            if (!schema.TryGetProperty("Name", out var nameElement))
            {
                WriteError(new ErrorRecord(
                    new Exception("Invalid schema format: missing Name property"),
                    "InvalidSchemaFormat",
                    ErrorCategory.InvalidData,
                    null));
                return;
            }
            var baseName = nameElement.GetString();

            if (Update)
            {
                // Update an existing base
                var updateBody = new
                {
                    name = baseName
                };

                var json = JsonSerializer.Serialize(updateBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    new Uri(TeableUrlBuilder.GetBaseUrl(baseId)))
                {
                    Content = content
                };

                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to update base {baseId}"),
                        "UpdateBaseFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                WriteVerbose($"Base {baseId} updated successfully");
            }
            else
            {
                // Create a new base
                var createBody = new
                {
                    name = baseName,
                    spaceId = SpaceId
                };

                var json = JsonSerializer.Serialize(createBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetBasesUrl(SpaceId)))
                {
                    Content = content
                };

                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableBase>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception("Failed to create base"),
                        "CreateBaseFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                baseId = response.Data.Id;
                WriteVerbose($"Base {baseId} created successfully");
            }

            // Process tables
            if (schema.TryGetProperty("Tables", out var tablesElement) && tablesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var tableElement in tablesElement.EnumerateArray())
                {
                    ImportTable(tableElement, baseId);
                }
            }

            // Process views
            if (schema.TryGetProperty("Views", out var viewsElement) && viewsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var viewElement in viewsElement.EnumerateArray())
                {
                    ImportView(viewElement);
                }
            }
        }

        private void ImportTable(JsonElement tableSchema, string baseId)
        {
            // Get the table ID and name
            string tableId = null;
            if (tableSchema.TryGetProperty("Id", out var idElement))
            {
                tableId = idElement.GetString();
            }

            if (!tableSchema.TryGetProperty("Name", out var nameElement))
            {
                WriteError(new ErrorRecord(
                    new Exception("Invalid table schema: missing Name property"),
                    "InvalidTableSchema",
                    ErrorCategory.InvalidData,
                    null));
                return;
            }
            var tableName = nameElement.GetString();

            if (Update && !string.IsNullOrEmpty(tableId))
            {
                // Update an existing table
                var updateBody = new
                {
                    name = tableName
                };

                var json = JsonSerializer.Serialize(updateBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    new Uri(TeableUrlBuilder.GetTableUrl(tableId)))
                {
                    Content = content
                };

                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to update table {tableId}"),
                        "UpdateTableFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                WriteVerbose($"Table {tableId} updated successfully");
            }
            else
            {
                // Create a new table
                var createBody = new
                {
                    name = tableName,
                    baseId = baseId
                };

                var json = JsonSerializer.Serialize(createBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetTablesUrl(baseId)))
                {
                    Content = content
                };

                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableTable>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception("Failed to create table"),
                        "CreateTableFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                tableId = response.Data.Id;
                WriteVerbose($"Table {tableId} created successfully");
            }

            // Process fields
            if (tableSchema.TryGetProperty("Fields", out var fieldsElement) && fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var fieldElement in fieldsElement.EnumerateArray())
                {
                    ImportField(fieldElement, tableId);
                }
            }
        }

        private void ImportField(JsonElement fieldSchema, string tableId)
        {
            // Get the field ID, name, and type
            string fieldId = null;
            if (fieldSchema.TryGetProperty("id", out var idElement))
            {
                fieldId = idElement.GetString();
            }

            if (!fieldSchema.TryGetProperty("name", out var nameElement) ||
                !fieldSchema.TryGetProperty("type", out var typeElement))
            {
                WriteError(new ErrorRecord(
                    new Exception("Invalid field schema: missing required properties"),
                    "InvalidFieldSchema",
                    ErrorCategory.InvalidData,
                    null));
                return;
            }

            var fieldName = nameElement.GetString();
            var fieldType = typeElement.GetString();

            // Create a field configuration based on the field type
            var options = new Dictionary<string, object>();
            if (fieldSchema.TryGetProperty("options", out var optionsElement) &&
                optionsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in optionsElement.EnumerateObject())
                {
                    options[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }

            if (Update && !string.IsNullOrEmpty(fieldId))
            {
                // Update an existing field
                var updateBody = new
                {
                    name = fieldName,
                    options = options
                };

                var json = JsonSerializer.Serialize(updateBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    new Uri(TeableUrlBuilder.GetFieldUrl(fieldId)))
                {
                    Content = content
                };

                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to update field {fieldId}"),
                        "UpdateFieldFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                WriteVerbose($"Field {fieldId} updated successfully");
            }
            else
            {
                // Create a new field
                var createBody = new
                {
                    name = fieldName,
                    type = fieldType,
                    options = options,
                    tableId = tableId
                };

                var json = JsonSerializer.Serialize(createBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetFieldsUrl(tableId)))
                {
                    Content = content
                };

                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableField>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception("Failed to create field"),
                        "CreateFieldFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                WriteVerbose($"Field {response.Data.Id} created successfully");
            }
        }

        private void ImportViewSchema(JsonElement schema)
        {
            // Check if we're updating an existing view or creating a new one
            string viewId = null;
            if (schema.TryGetProperty("Id", out var idElement))
            {
                viewId = idElement.GetString();
            }

            if (Update && string.IsNullOrEmpty(viewId))
            {
                WriteError(new ErrorRecord(
                    new Exception("Cannot update view: missing Id in schema"),
                    "MissingViewId",
                    ErrorCategory.InvalidData,
                    null));
                return;
            }

            // Get the view properties
            if (!schema.TryGetProperty("Name", out var nameElement) ||
                !schema.TryGetProperty("Type", out var typeElement) ||
                !schema.TryGetProperty("TableId", out var tableIdElement))
            {
                WriteError(new ErrorRecord(
                    new Exception("Invalid view schema: missing required properties"),
                    "InvalidViewSchema",
                    ErrorCategory.InvalidData,
                    null));
                return;
            }

            var viewName = nameElement.GetString();
            var viewType = typeElement.GetString();
            var tableId = tableIdElement.GetString();

            // Get filter and sort if available
            object filter = null;
            if (schema.TryGetProperty("Filter", out var filterElement) &&
                filterElement.ValueKind != JsonValueKind.Null)
            {
                filter = JsonSerializer.Deserialize<object>(filterElement.GetRawText());
            }

            object sort = null;
            if (schema.TryGetProperty("Sort", out var sortElement) &&
                sortElement.ValueKind != JsonValueKind.Null)
            {
                sort = JsonSerializer.Deserialize<object>(sortElement.GetRawText());
            }

            if (Update)
            {
                // Update an existing view
                var updateBody = new
                {
                    name = viewName,
                    filter = filter,
                    sort = sort
                };

                var json = JsonSerializer.Serialize(updateBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    new Uri(TeableUrlBuilder.GetViewUrl(viewId)))
                {
                    Content = content
                };

                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to update view {viewId}"),
                        "UpdateViewFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                WriteVerbose($"View {viewId} updated successfully");
            }
            else
            {
                // Create a new view
                var createBody = new
                {
                    name = viewName,
                    type = viewType,
                    filter = filter,
                    sort = sort,
                    tableId = tableId
                };

                var json = JsonSerializer.Serialize(createBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetViewsUrl(tableId)))
                {
                    Content = content
                };

                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableView>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception("Failed to create view"),
                        "CreateViewFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    return;
                }

                WriteVerbose($"View {response.Data.Id} created successfully");
            }
        }

        private void ImportView(JsonElement viewSchema)
        {
            // This method is used when importing views as part of a base schema
            ImportViewSchema(viewSchema);
        }
    }
}
