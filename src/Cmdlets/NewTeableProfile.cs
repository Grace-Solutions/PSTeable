using System;
using System.Management.Automation;
using System.Security;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Creates a new Teable connection profile
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableProfile")]
    [OutputType(typeof(TeableConnectionProfile))]
    public class NewTeableProfile : PSCmdlet
    {
        /// <summary>
        /// The name of the profile
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }
        
        /// <summary>
        /// The API token for the profile
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Token { get; set; }
        
        /// <summary>
        /// The base URL for the profile
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        public string BaseUrl { get; set; }
        
        /// <summary>
        /// Whether to connect to the profile after creating it
        /// </summary>
        [Parameter()]
        public SwitchParameter Connect { get; set; } = true;
        
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
                
                // Create a secure string from the token
                var secureToken = new SecureString();
                foreach (char c in Token)
                {
                    secureToken.AppendChar(c);
                }
                secureToken.MakeReadOnly();
                
                // Create the profile
                var profile = new TeableConnectionProfile
                {
                    Name = Name,
                    Token = secureToken,
                    BaseUrl = BaseUrl,
                    CreatedAt = DateTime.Now,
                    LastUsed = DateTime.Now
                };
                
                // Save the profile
                TeableProfileManager.SaveProfile(profile);
                
                // Connect to the profile if requested
                if (Connect)
                {
                    TeableSession.Instance.InitializeWithProfile(Name);
                }
                
                // Return the profile
                var outputProfile = new PSObject();
                outputProfile.Properties.Add(new PSNoteProperty("Name", profile.Name));
                outputProfile.Properties.Add(new PSNoteProperty("BaseUrl", profile.BaseUrl));
                outputProfile.Properties.Add(new PSNoteProperty("CreatedAt", profile.CreatedAt));
                outputProfile.Properties.Add(new PSNoteProperty("LastUsed", profile.LastUsed));
                
                WriteObject(outputProfile);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateProfileFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
