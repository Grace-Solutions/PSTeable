using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a Teable record payload for creating or updating records
    /// </summary>
    public class TeableRecordPayload
    {
        /// <summary>
        /// The fields of the record
        /// </summary>
        [JsonPropertyName("fields")]
        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
    }
}
