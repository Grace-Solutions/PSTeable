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
        /// The name of the profile to disconnect from
        /// </summary>
        [Parameter(Position = 0)]
        public string ProfileName { get; set; }

        /// <summary>
        /// Whether to disconnect from all profiles
        /// </summary>
        [Parameter()]
        public SwitchParameter All { get; set; }

        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (All)
                {
                    // Get all active connections
                    var connections = TeableSession.Instance.GetActiveConnections();

                    // Remove each connection
                    foreach (var connection in connections)
                    {
                        TeableSession.Instance.RemoveConnection(connection);
                        WriteVerbose($"Disconnected from profile '{connection}'");
                    }

                    // Clear the session
                    TeableSession.Instance.Clear();
                }
                else if (!string.IsNullOrEmpty(ProfileName))
                {
                    // Remove the specified connection
                    TeableSession.Instance.RemoveConnection(ProfileName);
                    WriteVerbose($"Disconnected from profile '{ProfileName}'");
                }
                else
                {
                    // Clear the current session
                    string currentProfile = TeableSession.Instance.CurrentProfileName;
                    TeableSession.Instance.Clear();

                    if (!string.IsNullOrEmpty(currentProfile))
                    {
                        WriteVerbose($"Disconnected from profile '{currentProfile}'");
                    }
                    else
                    {
                        WriteVerbose("Disconnected from Teable API");
                    }
                }
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

