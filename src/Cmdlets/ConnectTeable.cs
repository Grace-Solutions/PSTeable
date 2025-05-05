using System;
using System.Management.Automation;
using System.Security;
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
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ApiKey")]
        public string ApiKey { get; set; }

        /// <summary>
        /// The base URL of the Teable API
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ApiKey")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// The name of the profile to use
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Profile")]
        public string ProfileName { get; set; }

        /// <summary>
        /// Whether to save the connection as a profile
        /// </summary>
        [Parameter(ParameterSetName = "ApiKey")]
        public SwitchParameter SaveProfile { get; set; }

        /// <summary>
        /// The name to use when saving the profile
        /// </summary>
        [Parameter(ParameterSetName = "ApiKey")]
        public string Name { get; set; }

        /// <summary>
        /// Whether to force overwriting an existing profile
        /// </summary>
        [Parameter(ParameterSetName = "ApiKey")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ParameterSetName == "ApiKey")
                {
                    // Initialize the session with the API key and base URL
                    string profileName = SaveProfile ? (Name ?? "Default") : null;
                    TeableSession.Instance.Initialize(ApiKey, BaseUrl, profileName);

                    // If saving the profile, create a connection profile
                    if (SaveProfile)
                    {
                        // Check if the profile already exists
                        var existingProfile = TeableProfileManager.GetProfile(profileName);
                        if (existingProfile != null && !Force)
                        {
                            TeableSession.Instance.Clear();
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Profile '{profileName}' already exists. Use -Force to overwrite."),
                                "ProfileAlreadyExists",
                                ErrorCategory.ResourceExists,
                                profileName));
                        }

                        // Create a secure string from the API key
                        var secureApiKey = new SecureString();
                        foreach (char c in ApiKey)
                        {
                            secureApiKey.AppendChar(c);
                        }
                        secureApiKey.MakeReadOnly();

                        // Create and save the profile
                        var profile = new Models.TeableConnectionProfile
                        {
                            Name = profileName,
                            Token = secureApiKey,
                            BaseUrl = BaseUrl.TrimEnd('/'),
                            CreatedAt = DateTime.Now,
                            LastUsed = DateTime.Now
                        };

                        TeableProfileManager.SaveProfile(profile);
                        Logger.Verbose(this, $"Connection saved to profile '{profileName}'");
                    }
                }
                else // Profile
                {
                    // Initialize the session with the profile
                    TeableSession.Instance.InitializeWithProfile(ProfileName);
                }

                Logger.Verbose(this, $"Connected to Teable API at {TeableSession.Instance.BaseUrl}");

                // Test the connection
                var request = new System.Net.Http.HttpRequestMessage(
                    System.Net.Http.HttpMethod.Get,
                    TeableUrlBuilder.GetSpacesUrl());

                using var response = TeableSession.Instance.HttpClient.SendWithErrorHandling(request, this);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    TeableSession.Instance.Clear();
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Failed to connect to Teable API"),
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

