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
    /// Creates a new Teable base
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableBase")]
    [OutputType(typeof(TeableBase))]
    public class NewTeableBase : PSCmdlet
    {
        /// <summary>
        /// The ID of the space to create the base in
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string SpaceId { get; set; }

        /// <summary>
        /// The name of the base
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }

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
                    name = Name
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Create the request
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(TeableUrlBuilder.GetBasesUrl(SpaceId)))
                {
                    Content = content
                };

                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableBase>>(
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
                    "CreateBaseFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}




