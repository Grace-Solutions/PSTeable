using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable view
    /// </summary>
    public class TeableView
    {
        /// <summary>
        /// The ID of the view
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the view
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The type of the view
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The ID of the table that contains the view
        /// </summary>
        [JsonPropertyName("tableId")]
        public string TableId { get; set; }
        
        /// <summary>
        /// The filter expression for the view
        /// </summary>
        [JsonPropertyName("filter")]
        public object Filter { get; set; }
        
        /// <summary>
        /// The sort expression for the view
        /// </summary>
        [JsonPropertyName("sort")]
        public object Sort { get; set; }
        
        /// <summary>
        /// The creation time of the view
        /// </summary>
        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// The last modification time of the view
        /// </summary>
        [JsonPropertyName("lastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }
    }
}

