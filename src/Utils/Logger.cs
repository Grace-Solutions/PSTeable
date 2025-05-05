using System;
using System.Management.Automation;

namespace PSTeable.Utils
{
    /// <summary>
    /// Provides logging functionality for the module
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs a verbose message
        /// </summary>
        /// <param name="cmdlet">The cmdlet that is logging the message</param>
        /// <param name="message">The message to log</param>
        public static void Verbose(PSCmdlet cmdlet, string message)
        {
            cmdlet.WriteVerbose(message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="cmdlet">The cmdlet that is logging the message</param>
        /// <param name="message">The message to log</param>
        public static void Warning(PSCmdlet cmdlet, string message)
        {
            cmdlet.WriteWarning(message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="cmdlet">The cmdlet that is logging the message</param>
        /// <param name="message">The message to log</param>
        public static void Error(PSCmdlet cmdlet, string message)
        {
            cmdlet.WriteError(new ErrorRecord(
                new Exception(message),
                "TeableError",
                ErrorCategory.NotSpecified,
                null));
        }
    }
}

