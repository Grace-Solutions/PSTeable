using System;
using System.Management.Automation;
using System.Security;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Saves Teable API credentials to a profile
    /// </summary>
    [Cmdlet(VerbsData.Save, "TeableCredential")]
    [OutputType(typeof(void))]
    public class SaveTeableCredential : PSCmdlet
    {
        /// <summary>
        /// The name of the profile
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// The API token
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public SecureString Token { get; set; }

        /// <summary>
        /// The base URL of the Teable API
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        public string BaseUrl { get; set; }

        /// <summary>
        /// Whether to overwrite an existing profile
        /// </summary>
        [Parameter()]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Check if the profile already exists
                var existingProfile = TeableProfileManager.GetProfile(Name);
                if (existingProfile != null && !Force)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Profile '{Name}' already exists. Use -Force to overwrite."),
                        "ProfileAlreadyExists",
                        ErrorCategory.ResourceExists,
                        Name));
                    return;
                }

                // Create a new profile
                var profile = new TeableConnectionProfile
                {
                    Name = Name,
                    Token = Token,
                    BaseUrl = BaseUrl.TrimEnd('/'),
                    CreatedAt = DateTime.Now,
                    LastUsed = DateTime.Now
                };

                // Save the profile
                TeableProfileManager.SaveProfile(profile);

                Logger.Verbose(this, $"Credentials saved to profile '{Name}'");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SaveCredentialsFailed",
                    ErrorCategory.WriteError,
                    null));
            }
        }
    }
}
