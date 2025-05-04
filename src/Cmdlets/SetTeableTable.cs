using System;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;
using static PSTeable.Utils.HttpMethodExtensions;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Updates a Teable table
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "TeableTable")]
    [OutputType(typeof(TeableTable))]
    public class SetTeableTable : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to update
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }

        /// <summary>
        /// The new name of the table
        /// </summary>
        [Parameter(Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// The new description of the table
        /// </summary>
        [Parameter()]
        public string Description { get; set; }

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
                // Create the request body
                var bodyDict = new System.Collections.Generic.Dictionary<string, object>();

                if (!string.IsNullOrEmpty(Name))
                {
                    bodyDict.Add("name", Name);
                }

                if (Description != null)
                {
                    bodyDict.Add("description", Description);
                }

                if (bodyDict.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("At least one parameter to update must be specified"),
                        "MissingParameter",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                var json = JsonSerializer.Serialize(bodyDict);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Create the request
                var request = new HttpRequestMessage(
                    Patch,
                    new Uri(TeableUrlBuilder.GetTableUrl(TableId)))
                {
                    Content = content
                };

                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableTable>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);

                if (response?.Data != null)
                {
                    WriteObject(response.Data);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "UpdateTableFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


