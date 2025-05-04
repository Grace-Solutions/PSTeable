using System;
using System.Management.Automation;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Connects to the Teable API
    /// </summary>
    [Cmdlet(VerbsCommunications.Connect, "Teable")]
    [OutputType(typeof(void))]
    public class ConnectTeable : PSCmdlet
    {
        /// <summary>
        /// The API key to use for authentication
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string ApiKey { get; set; }
        
        /// <summary>
        /// The base URL of the Teable API
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string BaseUrl { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Initialize the session
                TeableSession.Instance.Initialize(ApiKey, BaseUrl);
                
                Logger.Verbose(this, $"Connected to Teable API at {BaseUrl}");
                
                // Test the connection
                var request = new System.Net.Http.HttpRequestMessage(
                    System.Net.Http.HttpMethod.Get,
                    TeableUrlBuilder.GetSpacesUrl());
                
                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(request, this);
                
                if (response == null || !response.IsSuccessStatusCode)
                {
                    TeableSession.Instance.Clear();
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception("Failed to connect to Teable API"),
                        "ConnectionFailed",
                        ErrorCategory.ConnectionError,
                        null));
                }
                
                WriteVerbose("Successfully connected to Teable API");
            }
            catch (Exception ex)
            {
                TeableSession.Instance.Clear();
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ConnectionFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}
