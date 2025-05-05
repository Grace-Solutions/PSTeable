using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a sort direction
    /// </summary>
    public enum TeableSortDirection
    {
        /// <summary>
        /// Ascending order
        /// </summary>
        [JsonPropertyName("asc")]
        Ascending,
        
        /// <summary>
        /// Descending order
        /// </summary>
        [JsonPropertyName("desc")]
        Descending
    }
    
    /// <summary>
    /// Represents a sort specification
    /// </summary>
    public class TeableSortSpec
    {
        /// <summary>
        /// The field ID to sort by
        /// </summary>
        [JsonPropertyName("fieldId")]
        public string FieldId { get; set; }
        
        /// <summary>
        /// The sort direction
        /// </summary>
        [JsonPropertyName("direction")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableSortDirection Direction { get; set; }
    }
    
    /// <summary>
    /// Represents a sort for Teable records
    /// </summary>
    public class TeableSort
    {
        /// <summary>
        /// The sort specifications
        /// </summary>
        private List<TeableSortSpec> _specs = new List<TeableSortSpec>();
        
        /// <summary>
        /// Creates a new sort with a single specification
        /// </summary>
        /// <param name="fieldId">The field ID to sort by</param>
        /// <param name="direction">The sort direction</param>
        public TeableSort(string fieldId, TeableSortDirection direction = TeableSortDirection.Ascending)
        {
            _specs.Add(new TeableSortSpec
            {
                FieldId = fieldId,
                Direction = direction
            });
        }
        
        /// <summary>
        /// Creates a new empty sort
        /// </summary>
        public TeableSort()
        {
        }
        
        /// <summary>
        /// Adds a sort specification
        /// </summary>
        /// <param name="fieldId">The field ID to sort by</param>
        /// <param name="direction">The sort direction</param>
        /// <returns>The sort</returns>
        public TeableSort AddSort(string fieldId, TeableSortDirection direction = TeableSortDirection.Ascending)
        {
            _specs.Add(new TeableSortSpec
            {
                FieldId = fieldId,
                Direction = direction
            });
            
            return this;
        }
        
        /// <summary>
        /// Converts the sort to a JSON string
        /// </summary>
        /// <returns>The JSON string</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(_specs);
        }
        
        /// <summary>
        /// Converts the sort to a query string parameter
        /// </summary>
        /// <returns>The query string parameter</returns>
        public string ToQueryString()
        {
            return Uri.EscapeDataString(ToJson());
        }
    }
}
