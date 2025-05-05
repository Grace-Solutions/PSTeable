using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents an automation trigger type
    /// </summary>
    public enum TeableAutomationTriggerType
    {
        /// <summary>
        /// Triggered when a record is created
        /// </summary>
        [JsonPropertyName("record.created")]
        RecordCreated,
        
        /// <summary>
        /// Triggered when a record is updated
        /// </summary>
        [JsonPropertyName("record.updated")]
        RecordUpdated,
        
        /// <summary>
        /// Triggered when a record matches a condition
        /// </summary>
        [JsonPropertyName("record.matches")]
        RecordMatches,
        
        /// <summary>
        /// Triggered on a schedule
        /// </summary>
        [JsonPropertyName("schedule")]
        Schedule
    }
    
    /// <summary>
    /// Represents an automation action type
    /// </summary>
    public enum TeableAutomationActionType
    {
        /// <summary>
        /// Sends an email
        /// </summary>
        [JsonPropertyName("email")]
        Email,
        
        /// <summary>
        /// Makes an HTTP request
        /// </summary>
        [JsonPropertyName("http")]
        Http,
        
        /// <summary>
        /// Updates a record
        /// </summary>
        [JsonPropertyName("update")]
        Update,
        
        /// <summary>
        /// Creates a record
        /// </summary>
        [JsonPropertyName("create")]
        Create
    }
    
    /// <summary>
    /// Represents an automation trigger
    /// </summary>
    public class TeableAutomationTrigger
    {
        /// <summary>
        /// The type of trigger
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableAutomationTriggerType Type { get; set; }
        
        /// <summary>
        /// The configuration for the trigger
        /// </summary>
        [JsonPropertyName("config")]
        public object Config { get; set; }
    }
    
    /// <summary>
    /// Represents an automation action
    /// </summary>
    public class TeableAutomationAction
    {
        /// <summary>
        /// The type of action
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableAutomationActionType Type { get; set; }
        
        /// <summary>
        /// The configuration for the action
        /// </summary>
        [JsonPropertyName("config")]
        public object Config { get; set; }
    }
    
    /// <summary>
    /// Represents a Teable automation
    /// </summary>
    public class TeableAutomation
    {
        /// <summary>
        /// The ID of the automation
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the automation
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The ID of the table the automation is for
        /// </summary>
        [JsonPropertyName("tableId")]
        public string TableId { get; set; }
        
        /// <summary>
        /// The trigger for the automation
        /// </summary>
        [JsonPropertyName("trigger")]
        public TeableAutomationTrigger Trigger { get; set; }
        
        /// <summary>
        /// The actions for the automation
        /// </summary>
        [JsonPropertyName("actions")]
        public TeableAutomationAction[] Actions { get; set; }
        
        /// <summary>
        /// Whether the automation is enabled
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        /// <summary>
        /// The date and time when the automation was created
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the automation was last updated
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
