using System;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable spaces
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableSpace")]
    [OutputType(typeof(TeableSpace))]
    public class GetTeableSpace : PSCmdlet
    {
        /// <summary>
        /// The ID of the space to get
        /// </summary>
        [Parameter(Position = 0)]
        public string SpaceId { get; set; }

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
                if (string.IsNullOrEmpty(SpaceId))
                {
                    // Get all spaces
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetSpacesUrl()));

                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableSpace>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    if (response?.Data != null)
                    {
                        foreach (var space in response.Data)
                        {
                            WriteObject(space);
                        }
                    }
                }
                else
                {
                    // Get a specific space
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        new Uri(TeableUrlBuilder.GetSpaceUrl(SpaceId)));

                    var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableSpace>>(
                        request,
                        this,
                        RespectRateLimit,
                        RateLimitDelay);

                    if (response?.Data != null)
                    {
                        WriteObject(response.Data);
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetSpaceFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


