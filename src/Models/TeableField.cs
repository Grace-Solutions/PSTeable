using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable field
    /// </summary>
    public class TeableField
    {
        /// <summary>
        /// The ID of the field
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the field
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The type of the field
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The ID of the table that contains the field
        /// </summary>
        [JsonPropertyName("tableId")]
        public string TableId { get; set; }
        
        /// <summary>
        /// Whether the field is a primary field
        /// </summary>
        [JsonPropertyName("isPrimary")]
        public bool IsPrimary { get; set; }
        
        /// <summary>
        /// Whether the field is a system field
        /// </summary>
        [JsonPropertyName("isSystem")]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// The options for the field
        /// </summary>
        [JsonPropertyName("options")]
        public object Options { get; set; }
        
        /// <summary>
        /// The creation time of the field
        /// </summary>
        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// The last modification time of the field
        /// </summary>
        [JsonPropertyName("lastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }
    }
}

