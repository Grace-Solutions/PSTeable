using System;
using System.Collections;
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
    /// Updates a Teable view
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "TeableView")]
    [OutputType(typeof(TeableView))]
    public class SetTeableView : PSCmdlet
    {
        /// <summary>
        /// The ID of the view to update
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string ViewId { get; set; }

        /// <summary>
        /// The new name of the view
        /// </summary>
        [Parameter(Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// The new filter for the view
        /// </summary>
        [Parameter()]
        public Hashtable Filter { get; set; }

        /// <summary>
        /// The new sort for the view
        /// </summary>
        [Parameter()]
        public Hashtable Sort { get; set; }

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

                if (Filter != null)
                {
                    var filterDict = new System.Collections.Generic.Dictionary<string, object>();
                    foreach (DictionaryEntry entry in Filter)
                    {
                        filterDict.Add(entry.Key.ToString(), entry.Value);
                    }

                    bodyDict.Add("filter", filterDict);
                }

                if (Sort != null)
                {
                    var sortDict = new System.Collections.Generic.Dictionary<string, object>();
                    foreach (DictionaryEntry entry in Sort)
                    {
                        sortDict.Add(entry.Key.ToString(), entry.Value);
                    }

                    bodyDict.Add("sort", sortDict);
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
                    new Uri(TeableUrlBuilder.GetViewUrl(ViewId)))
                {
                    Content = content
                };

                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableView>>(
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
                    "UpdateViewFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}


