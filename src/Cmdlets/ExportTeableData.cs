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
    /// Exports data from a Teable table
    /// </summary>
    [Cmdlet(VerbsData.Export, "TeableData")]
    [OutputType(typeof(void))]
    public class ExportTeableData : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to export data from
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }

        /// <summary>
        /// The path to save the data to
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; }

        /// <summary>
        /// The filter to apply to the records
        /// </summary>
        [Parameter()]
        public string Filter { get; set; }

        /// <summary>
        /// The fields to include in the export
        /// </summary>
        [Parameter()]
        public string[] Fields { get; set; }

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
                // Build the fields string
                string fieldsString = null;
                if (Fields != null && Fields.Length > 0)
                {
                    fieldsString = string.Join(",", Fields);
                }

                // Get records with pagination
                var records = new List<TeableRecord>();
                string pageToken = null;

                do
                {
                    var url = TeableUrlBuilder.GetRecordsUrl(
                        TableId,
                        null,
                        Filter,
                        null,
                        fieldsString,
                        100, // Page size
                        pageToken);

                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

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

                // Save the records to a file
                var json = JsonSerializer.Serialize(records, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(Path, json);

                WriteVerbose($"Exported {records.Count} records to {Path}");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ExportDataFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


