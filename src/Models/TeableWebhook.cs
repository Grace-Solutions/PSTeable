using System;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a webhook event type
    /// </summary>
    public enum TeableWebhookEventType
    {
        /// <summary>
        /// A record was created
        /// </summary>
        [JsonPropertyName("record.created")]
        RecordCreated,
        
        /// <summary>
        /// A record was updated
        /// </summary>
        [JsonPropertyName("record.updated")]
        RecordUpdated,
        
        /// <summary>
        /// A record was deleted
        /// </summary>
        [JsonPropertyName("record.deleted")]
        RecordDeleted,
        
        /// <summary>
        /// A comment was created
        /// </summary>
        [JsonPropertyName("comment.created")]
        CommentCreated,
        
        /// <summary>
        /// A field was created
        /// </summary>
        [JsonPropertyName("field.created")]
        FieldCreated,
        
        /// <summary>
        /// A field was updated
        /// </summary>
        [JsonPropertyName("field.updated")]
        FieldUpdated,
        
        /// <summary>
        /// A field was deleted
        /// </summary>
        [JsonPropertyName("field.deleted")]
        FieldDeleted
    }
    
    /// <summary>
    /// Represents a Teable webhook
    /// </summary>
    public class TeableWebhook
    {
        /// <summary>
        /// The ID of the webhook
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the webhook
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The URL to send webhook events to
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }
        
        /// <summary>
        /// The ID of the table the webhook is for
        /// </summary>
        [JsonPropertyName("tableId")]
        public string TableId { get; set; }
        
        /// <summary>
        /// The event types to trigger the webhook
        /// </summary>
        [JsonPropertyName("eventTypes")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableWebhookEventType[] EventTypes { get; set; }
        
        /// <summary>
        /// Whether the webhook is enabled
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        /// <summary>
        /// The secret used to sign webhook payloads
        /// </summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; }
        
        /// <summary>
        /// The date and time when the webhook was created
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the webhook was last updated
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
