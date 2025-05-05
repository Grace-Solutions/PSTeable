using System;
using System.Management.Automation;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable connection profiles
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableProfile")]
    [OutputType(typeof(TeableConnectionProfile))]
    public class GetTeableProfile : PSCmdlet
    {
        /// <summary>
        /// The name of the profile to get
        /// </summary>
        [Parameter(Position = 0)]
        public string Name { get; set; }
        
        /// <summary>
        /// Whether to include the token in the output
        /// </summary>
        [Parameter()]
        public SwitchParameter IncludeToken { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (string.IsNullOrEmpty(Name))
                {
                    // Get all profiles
                    var profiles = TeableProfileManager.GetAllProfiles();
                    
                    foreach (var profile in profiles)
                    {
                        WriteProfile(profile);
                    }
                }
                else
                {
                    // Get a specific profile
                    var profile = TeableProfileManager.GetProfile(Name);
                    
                    if (profile != null)
                    {
                        WriteProfile(profile);
                    }
                    else
                    {
                        WriteError(new ErrorRecord(
                            new ItemNotFoundException($"Profile '{Name}' not found"),
                            "ProfileNotFound",
                            ErrorCategory.ObjectNotFound,
                            Name));
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetProfileFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Writes a profile to the output
        /// </summary>
        /// <param name="profile">The profile to write</param>
        private void WriteProfile(TeableConnectionProfile profile)
        {
            if (!IncludeToken)
            {
                // Create a copy of the profile without the token
                var outputProfile = new PSObject();
                outputProfile.Properties.Add(new PSNoteProperty("Name", profile.Name));
                outputProfile.Properties.Add(new PSNoteProperty("BaseUrl", profile.BaseUrl));
                outputProfile.Properties.Add(new PSNoteProperty("CreatedAt", profile.CreatedAt));
                outputProfile.Properties.Add(new PSNoteProperty("LastUsed", profile.LastUsed));
                
                WriteObject(outputProfile);
            }
            else
            {
                WriteObject(profile);
            }
        }
    }
}
