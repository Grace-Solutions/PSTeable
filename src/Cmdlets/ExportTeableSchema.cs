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
    /// Exports the schema of a Teable base
    /// </summary>
    [Cmdlet(VerbsData.Export, "TeableSchema")]
    [OutputType(typeof(void))]
    public class ExportTeableSchema : PSCmdlet
    {
        /// <summary>
        /// The ID of the base to export the schema from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string BaseId { get; set; }

        /// <summary>
        /// The path to save the schema to
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; }

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
                    baseResponse.Data.Id,
                    baseResponse.Data.Name,
                    baseResponse.Data.SpaceId,
                    Tables = new List<object>()
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
                }

                // Save the schema to a file
                var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(Path, json);

                WriteVerbose($"Schema exported to {Path}");
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
    }
}


