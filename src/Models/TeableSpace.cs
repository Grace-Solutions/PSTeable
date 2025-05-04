using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable space
    /// </summary>
    public class TeableSpace
    {
        /// <summary>
        /// The ID of the space
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the space
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The creation time of the space
        /// </summary>
        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// The last modification time of the space
        /// </summary>
        [JsonPropertyName("lastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }
    }
}
