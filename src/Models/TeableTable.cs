using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable table
    /// </summary>
    public class TeableTable
    {
        /// <summary>
        /// The ID of the table
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the table
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The ID of the base that contains the table
        /// </summary>
        [JsonPropertyName("baseId")]
        public string BaseId { get; set; }
        
        /// <summary>
        /// The creation time of the table
        /// </summary>
        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// The last modification time of the table
        /// </summary>
        [JsonPropertyName("lastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }
    }
}

