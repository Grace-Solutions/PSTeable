using System;
using System.Security;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable connection profile
    /// </summary>
    public class TeableConnectionProfile
    {
        /// <summary>
        /// The name of the connection profile
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The API token for the connection
        /// </summary>
        public SecureString Token { get; set; }
        
        /// <summary>
        /// The base URL for the Teable API
        /// </summary>
        public string BaseUrl { get; set; }
        
        /// <summary>
        /// The date and time when the profile was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the profile was last used
        /// </summary>
        public DateTime LastUsed { get; set; }
    }
}
