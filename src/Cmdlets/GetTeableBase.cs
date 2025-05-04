using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable bases
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableBase")]
    [OutputType(typeof(TeableBase))]
    public class GetTeableBase : PSCmdlet
    {
        /// <summary>
        /// The ID of the space to get bases from
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "BySpace")]
        public string SpaceId { get; set; }

        /// <summary>
        /// The ID of the base to get
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByBase")]
        public string BaseId { get; set; }

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
                if (!string.IsNullOrEmpty(SpaceId))
                {
                    // Get all bases in a space
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetBasesUrl(SpaceId)));

                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableBase>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    if (response?.Data != null)
                    {
                        foreach (var baseObj in response.Data)
                        {
                            WriteObject(baseObj);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(BaseId))
                {
                    // Get a specific base
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetBaseUrl(BaseId)));

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
                else
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Either SpaceId or BaseId must be specified"),
                        "MissingParameter",
                        ErrorCategory.InvalidArgument,
                        null));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetBaseFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}



