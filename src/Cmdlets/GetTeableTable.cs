using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable tables
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableTable")]
    [OutputType(typeof(TeableTable))]
    public class GetTeableTable : PSCmdlet
    {
        /// <summary>
        /// The ID of the base to get tables from
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByBase")]
        public string BaseId { get; set; }

        /// <summary>
        /// The ID of the table to get
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByTable")]
        public string TableId { get; set; }

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
                if (!string.IsNullOrEmpty(BaseId))
                {
                    // Get all tables in a base
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetTablesUrl(BaseId)));

                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableTable>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    if (response?.Data != null)
                    {
                        foreach (var table in response.Data)
                        {
                            WriteObject(table);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(TableId))
                {
                    // Get a specific table
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetTableUrl(TableId)));

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
                else
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Either BaseId or TableId must be specified"),
                        "MissingParameter",
                        ErrorCategory.InvalidArgument,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetTableFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}



