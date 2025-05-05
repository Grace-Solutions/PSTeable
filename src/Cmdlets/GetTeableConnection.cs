using System;
using System.Management.Automation;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Gets information about the current Teable connection
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TeableConnection")]
    [OutputType(typeof(PSObject))]
    public class GetTeableConnection : PSCmdlet
    {
        /// <summary>
        /// Whether to list all active connections
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
                    
                    foreach (var connection in connections)
                    {
                        // Get the connection details
                        var profile = TeableProfileManager.GetProfile(connection);
                        if (profile != null)
                        {
                            var result = new PSObject();
                            result.Properties.Add(new PSNoteProperty("Name", profile.Name));
                            result.Properties.Add(new PSNoteProperty("BaseUrl", profile.BaseUrl));
                            result.Properties.Add(new PSNoteProperty("CreatedAt", profile.CreatedAt));
                            result.Properties.Add(new PSNoteProperty("LastUsed", profile.LastUsed));
                            result.Properties.Add(new PSNoteProperty("IsCurrent", profile.Name == TeableSession.Instance.CurrentProfileName));
                            
                            WriteObject(result);
                        }
                    }
                }
                else
                {
                    // Get the current connection
                    if (!TeableSession.Instance.IsConnected)
                    {
                        WriteWarning("Not connected to Teable API");
                        return;
                    }
                    
                    var result = new PSObject();
                    result.Properties.Add(new PSNoteProperty("BaseUrl", TeableSession.Instance.BaseUrl));
                    result.Properties.Add(new PSNoteProperty("ProfileName", TeableSession.Instance.CurrentProfileName));
                    result.Properties.Add(new PSNoteProperty("IsConnected", TeableSession.Instance.IsConnected));
                    
                    WriteObject(result);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GetConnectionFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
