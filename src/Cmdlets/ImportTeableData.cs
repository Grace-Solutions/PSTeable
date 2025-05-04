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
    /// Imports data into a Teable table
    /// </summary>
    [Cmdlet(VerbsData.Import, "TeableData")]
    [OutputType(typeof(void))]
    public class ImportTeableData : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to import data into
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }

        /// <summary>
        /// The path to the JSON file containing the data to import
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
                // Read the data from the file
                var json = File.ReadAllText(Path);
                var records = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);

                if (records == null || records.Count == 0)
                {
                    WriteWarning("No records found in the file");
                    return;
                }

                // Import the records
                var recordPayloads = new List<TeableRecordPayload>();

                foreach (var record in records)
                {
                    var payload = new TeableRecordPayload
                    {
                        Fields = record
                    };

                    recordPayloads.Add(payload);
                }

                // Create the request body
                var body = new
                {
                    records = recordPayloads
                };

                var requestJson = JsonSerializer.Serialize(body);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
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

                if (response != null && response.IsSuccessStatusCode)
                {
                    WriteVerbose($"Imported {records.Count} records into table {TableId}");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ImportDataFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}




