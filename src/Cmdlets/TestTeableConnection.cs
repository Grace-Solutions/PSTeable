using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Net.Http;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Tests the connection to the Teable API
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "TeableConnection")]
    [OutputType(typeof(bool))]
    public class TestTeableConnection : PSCmdlet
    {
        /// <summary>
        /// The name of the profile to test
        /// </summary>
        [Parameter(Position = 0)]
        public string ProfileName { get; set; }

        /// <summary>
        /// Whether to return detailed information
        /// </summary>
        [Parameter()]
        public SwitchParameter Detailed { get; set; }

        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                bool success;
                string message;
                TimeSpan latency = TimeSpan.Zero;

                // If a profile name is provided, switch to that connection
                if (!string.IsNullOrEmpty(ProfileName))
                {
                    try
                    {
                        // Check if the profile exists
                        var profile = TeableProfileManager.GetProfile(ProfileName);
                        if (profile == null)
                        {
                            WriteError(new ErrorRecord(
                                new ItemNotFoundException($"Profile '{ProfileName}' not found"),
                                "ProfileNotFound",
                                ErrorCategory.ObjectNotFound,
                                ProfileName));
                            return;
                        }

                        // Switch to the profile
                        TeableSession.Instance.SwitchConnection(ProfileName);

                        // Test the connection
                        (success, message, latency) = TestConnection();
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        message = $"Failed to switch to profile '{ProfileName}': {ex.Message}";
                    }
                }
                else
                {
                    // Test the current connection
                    if (!TeableSession.Instance.IsConnected)
                    {
                        WriteError(new ErrorRecord(
                            new InvalidOperationException("Not connected to Teable API"),
                            "NotConnected",
                            ErrorCategory.ConnectionError,
                            null));
                        return;
                    }

                    // Test the connection
                    (success, message, latency) = TestConnection();
                }

                if (Detailed)
                {
                    // Return detailed results
                    var result = new PSObject();
                    result.Properties.Add(new PSNoteProperty("Success", success));
                    result.Properties.Add(new PSNoteProperty("Message", message));
                    result.Properties.Add(new PSNoteProperty("Latency", latency));
                    result.Properties.Add(new PSNoteProperty("BaseUrl", TeableSession.Instance.BaseUrl));
                    result.Properties.Add(new PSNoteProperty("ProfileName", TeableSession.Instance.CurrentProfileName));

                    WriteObject(result);
                }
                else
                {
                    // Return a simple boolean
                    WriteObject(success);
                }
            }
            catch (Exception ex)
            {
                if (Detailed)
                {
                    var result = new PSObject();
                    result.Properties.Add(new PSNoteProperty("Success", false));
                    result.Properties.Add(new PSNoteProperty("Message", $"Connection test failed: {ex.Message}"));
                    result.Properties.Add(new PSNoteProperty("Exception", ex));

                    WriteObject(result);
                }
                else
                {
                    WriteObject(false);
                }
            }
        }

        /// <summary>
        /// Tests the current connection
        /// </summary>
        /// <returns>A tuple containing the success status, a message, and the latency</returns>
        private (bool Success, string Message, TimeSpan Latency) TestConnection()
        {
            try
            {
                // Create a request to the spaces endpoint
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(TeableUrlBuilder.GetSpacesUrl()));

                // Measure the latency
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAsync(request).GetAwaiter().GetResult();

                stopwatch.Stop();

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Connection successful", stopwatch.Elapsed);
                }
                else
                {
                    return (false, $"Connection failed: {response.StatusCode}", stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Connection failed: {ex.Message}", TimeSpan.Zero);
            }
        }
    }
}
