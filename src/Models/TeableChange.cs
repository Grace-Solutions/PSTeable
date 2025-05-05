using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents the type of change
    /// </summary>
    public enum TeableChangeType
    {
        /// <summary>
        /// A record was created
        /// </summary>
        [JsonPropertyName("create")]
        Create,
        
        /// <summary>
        /// A record was updated
        /// </summary>
        [JsonPropertyName("update")]
        Update,
        
        /// <summary>
        /// A record was deleted
        /// </summary>
        [JsonPropertyName("delete")]
        Delete
    }
    
    /// <summary>
    /// Represents a change to a record
    /// </summary>
    public class TeableChange
    {
        /// <summary>
        /// The ID of the record
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The type of change
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableChangeType Type { get; set; }
        
        /// <summary>
        /// The record data (for create and update)
        /// </summary>
        [JsonPropertyName("record")]
        public TeableRecord Record { get; set; }
        
        /// <summary>
        /// The timestamp of the change
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// The user who made the change
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }
}
