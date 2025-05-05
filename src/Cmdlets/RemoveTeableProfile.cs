using System;
using System.Management.Automation;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable connection profile
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableProfile", SupportsShouldProcess = true)]
    [OutputType(typeof(void))]
    public class RemoveTeableProfile : PSCmdlet
    {
        /// <summary>
        /// The name of the profile to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Check if the profile exists
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
                
                // Confirm the removal
                if (!ShouldProcess(Name, "Remove profile"))
                {
                    return;
                }
                
                // Remove the profile
                bool removed = TeableProfileManager.RemoveProfile(Name);
                
                if (removed)
                {
                    // If this is the current profile, disconnect from it
                    if (TeableSession.Instance.CurrentProfileName == Name)
                    {
                        TeableSession.Instance.RemoveConnection(Name);
                    }
                    
                    WriteVerbose($"Profile '{Name}' removed successfully");
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to remove profile '{Name}'"),
                        "RemoveProfileFailed",
                        ErrorCategory.InvalidOperation,
                        Name));
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveProfileFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
