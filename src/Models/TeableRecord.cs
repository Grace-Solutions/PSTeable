using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable record
    /// </summary>
    public class TeableRecord
    {
        /// <summary>
        /// The ID of the record
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The fields of the record
        /// </summary>
        [JsonPropertyName("fields")]
        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// The creation time of the record
        /// </summary>
        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// The last modification time of the record
        /// </summary>
        [JsonPropertyName("lastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }
    }
}
