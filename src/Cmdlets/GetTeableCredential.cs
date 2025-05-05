using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets Teable API credentials from a profile
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableCredential")]
    [OutputType(typeof(TeableConnectionProfile))]
    public class GetTeableCredential : PSCmdlet
    {
        /// <summary>
        /// The name of the profile
        /// </summary>
        [Parameter(Position = 0)]
        public string Name { get; set; }
        
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
                        WriteObject(profile);
                    }
                }
                else
                {
                    // Get a specific profile
                    var profile = TeableProfileManager.GetProfile(Name);
                    if (profile == null)
                    {
                        WriteError(new ErrorRecord(
                            new ItemNotFoundException($"Profile '{Name}' not found"),
                            "ProfileNotFound",
                            ErrorCategory.ObjectNotFound,
                            Name));
                        return;
                    }
                    
                    WriteObject(profile);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetCredentialsFailed",
                    ErrorCategory.ReadError,
                    null));
            }
        }
    }
}
