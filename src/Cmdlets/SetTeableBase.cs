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
    /// Updates a Teable base
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "TeableBase")]
    [OutputType(typeof(TeableBase))]
    public class SetTeableBase : PSCmdlet
    {
        /// <summary>
        /// The ID of the base to update
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string BaseId { get; set; }

        /// <summary>
        /// The new name of the base
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
                    Patch,
                    new Uri(TeableUrlBuilder.GetBaseUrl(BaseId)))
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
                    "UpdateBaseFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}



