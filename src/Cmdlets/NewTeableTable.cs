using System;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Creates a new Teable table
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableTable")]
    [OutputType(typeof(TeableTable))]
    public class NewTeableTable : PSCmdlet
    {
        /// <summary>
        /// The ID of the base to create the table in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string BaseId { get; set; }

        /// <summary>
        /// The name of the table
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// The description of the table
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
                var body = new
                {
                    name = Name,
                    description = Description
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetTablesUrl(BaseId)))
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
                    "CreateTableFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}




