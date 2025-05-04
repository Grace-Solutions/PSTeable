using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Exports the schema of a Teable base or view
    /// </summary>
    [Cmdlet(VerbsData.Export, "TeableSchema")]
    [OutputType(typeof(void))]
    public class ExportTeableSchema : PSCmdlet
    {
        /// <summary>
        /// The ID of the base to export the schema from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Base")]
        public string BaseId { get; set; }

        /// <summary>
        /// The ID of the view to export the schema from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "View")]
        public string ViewId { get; set; }

        /// <summary>
        /// The path to save the schema to
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; }

        /// <summary>
        /// Whether to include views in the schema export (only applicable when exporting a base)
        /// </summary>
        [Parameter(ParameterSetName = "Base")]
        public SwitchParameter IncludeViews { get; set; }

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
                if (ParameterSetName == "Base")
                {
                    ExportBaseSchema();
                }
                else if (ParameterSetName == "View")
                {
                    ExportViewSchema();
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ExportSchemaFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }

        private void ExportBaseSchema()
        {
            // Get the base
            var baseRequest = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetBaseUrl(BaseId)));

            var baseResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableBase>>(
                baseRequest,
                this,
                RespectRateLimit,
                RateLimitDelay);

            if (baseResponse?.Data == null)
            {
                WriteError(new ErrorRecord(
                    new Exception($"Base {BaseId} not found"),
                    "BaseNotFound",
                    ErrorCategory.ObjectNotFound,
                    null));
                return;
            }

            // Get the tables
            var tablesRequest = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetTablesUrl(BaseId)));

            var tablesResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableTable>>(
                tablesRequest,
                this,
                RespectRateLimit,
                RateLimitDelay);

            if (tablesResponse?.Data == null)
            {
                WriteError(new ErrorRecord(
                    new Exception($"Failed to get tables for base {BaseId}"),
                    "GetTablesFailed",
                    ErrorCategory.ConnectionError,
                    null));
                return;
            }

            // Get the fields for each table
            var schema = new
            {
                Type = "Base",
                baseResponse.Data.Id,
                baseResponse.Data.Name,
                baseResponse.Data.SpaceId,
                Tables = new List<object>(),
                Views = IncludeViews ? new List<object>() : null
            };

            foreach (var table in tablesResponse.Data)
            {
                var fieldsRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(TeableUrlBuilder.GetFieldsUrl(table.Id)));

                var fieldsResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                    fieldsRequest,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (fieldsResponse?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to get fields for table {table.Id}"),
                        "GetFieldsFailed",
                        ErrorCategory.ConnectionError,
                        null));
                    continue;
                }

                var tableSchema = new
                {
                    table.Id,
                    table.Name,
                    Fields = fieldsResponse.Data
                };

                schema.Tables.Add(tableSchema);

                // Get views if requested
                if (IncludeViews)
                {
                    var viewsRequest = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetViewsUrl(table.Id)));

                    var viewsResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableView>>(
                        viewsRequest,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    if (viewsResponse?.Data != null)
                    {
                        foreach (var view in viewsResponse.Data)
                        {
                            var viewSchema = new
                            {
                                view.Id,
                                view.Name,
                                view.Type,
                                view.TableId,
                                view.Filter,
                                view.Sort
                            };

                            schema.Views.Add(viewSchema);
                        }
                    }
                }
            }

            // Save the schema to a file
            var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(Path, json);

            WriteVerbose($"Base schema exported to {Path}");
        }

        private void ExportViewSchema()
        {
            // Get the view
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

            var view = viewResponse.Data;

            // Get the table
            var tableRequest = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetTableUrl(view.TableId)));

            var tableResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableTable>>(
                tableRequest,
                this,
                RespectRateLimit,
                RateLimitDelay);

            if (tableResponse?.Data == null)
            {
                WriteError(new ErrorRecord(
                    new Exception($"Table {view.TableId} not found"),
                    "TableNotFound",
                    ErrorCategory.ObjectNotFound,
                    null));
                return;
            }

            // Get the fields
            var fieldsRequest = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(TeableUrlBuilder.GetFieldsUrl(view.TableId)));

            var fieldsResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                fieldsRequest,
                this,
                RespectRateLimit,
                RateLimitDelay);

            if (fieldsResponse?.Data == null)
            {
                WriteError(new ErrorRecord(
                    new Exception($"Failed to get fields for table {view.TableId}"),
                    "GetFieldsFailed",
                    ErrorCategory.ConnectionError,
                    null));
                return;
            }

            // Create the schema
            var schema = new
            {
                Type = "View",
                Id = view.Id,
                Name = view.Name,
                ViewType = view.Type,
                TableId = view.TableId,
                TableName = tableResponse.Data.Name,
                Filter = view.Filter,
                Sort = view.Sort,
                Fields = fieldsResponse.Data
            };

            // Save the schema to a file
            var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(Path, json);

            WriteVerbose($"View schema exported to {Path}");
        }

    }
}



