using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a generic Teable API response
    /// </summary>
    /// <typeparam name="T">The type of the data in the response</typeparam>
    public class TeableResponse<T>
    {
        /// <summary>
        /// The data in the response
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }
        
        /// <summary>
        /// The pagination token for the next page
        /// </summary>
        [JsonPropertyName("nextPageToken")]
        public string NextPageToken { get; set; }
    }
    
    /// <summary>
    /// Represents a Teable API response with a list of items
    /// </summary>
    /// <typeparam name="T">The type of the items in the list</typeparam>
    public class TeableListResponse<T>
    {
        /// <summary>
        /// The items in the response
        /// </summary>
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
        
        /// <summary>
        /// The pagination token for the next page
        /// </summary>
        [JsonPropertyName("nextPageToken")]
        public string NextPageToken { get; set; }
    }
}
