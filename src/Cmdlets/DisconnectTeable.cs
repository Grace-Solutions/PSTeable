using System;
using System.Management.Automation;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Disconnects from the Teable API
    /// </summary>
    [Cmdlet(VerbsCommunications.Disconnect, "Teable")]
    [OutputType(typeof(void))]
    public class DisconnectTeable : PSCmdlet
    {
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Clear the session
                TeableSession.Instance.Clear();
                
                WriteVerbose("Disconnected from Teable API");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "DisconnectionFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }
    }
}

