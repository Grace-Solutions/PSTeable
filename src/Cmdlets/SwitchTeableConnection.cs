using System;
using System.Management.Automation;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Switches to a different Teable API connection
    /// </summary>
    [Cmdlet(VerbsCommon.Switch, "TeableConnection")]
    [OutputType(typeof(void))]
    public class SwitchTeableConnection : PSCmdlet
    {
        /// <summary>
        /// The name of the profile to switch to
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string ProfileName { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
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
                
                WriteVerbose($"Switched to connection profile '{ProfileName}'");
                
                // Test the connection
                bool success = TeableSession.Instance.TestConnection();
                if (!success)
                {
                    WriteWarning($"Connection to profile '{ProfileName}' may not be valid. Use Test-TeableConnection for more details.");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SwitchConnectionFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}
