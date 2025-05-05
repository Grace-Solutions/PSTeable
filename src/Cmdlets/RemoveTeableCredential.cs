using System;
using System.Management.Automation;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Removes a Teable API credential profile
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "TeableCredential", SupportsShouldProcess = true)]
    [OutputType(typeof(void))]
    public class RemoveTeableCredential : PSCmdlet
    {
        /// <summary>
        /// The name of the profile
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
                if (!ShouldProcess(Name, "Remove credential profile"))
                {
                    return;
                }
                
                // Remove the profile
                TeableProfileManager.RemoveProfile(Name);
                
                // Also remove the connection if it exists
                TeableSession.Instance.RemoveConnection(Name);
                
                WriteVerbose($"Credential profile '{Name}' removed");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "RemoveCredentialsFailed",
                    ErrorCategory.WriteError,
                    null));
            }
        }
    }
}
