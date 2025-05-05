using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents a filter operator
    /// </summary>
    public enum TeableFilterOperator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        [JsonPropertyName("eq")]
        Equal,
        
        /// <summary>
        /// Not equal to
        /// </summary>
        [JsonPropertyName("neq")]
        NotEqual,
        
        /// <summary>
        /// Greater than
        /// </summary>
        [JsonPropertyName("gt")]
        GreaterThan,
        
        /// <summary>
        /// Greater than or equal to
        /// </summary>
        [JsonPropertyName("gte")]
        GreaterThanOrEqual,
        
        /// <summary>
        /// Less than
        /// </summary>
        [JsonPropertyName("lt")]
        LessThan,
        
        /// <summary>
        /// Less than or equal to
        /// </summary>
        [JsonPropertyName("lte")]
        LessThanOrEqual,
        
        /// <summary>
        /// Contains
        /// </summary>
        [JsonPropertyName("contains")]
        Contains,
        
        /// <summary>
        /// Does not contain
        /// </summary>
        [JsonPropertyName("notContains")]
        NotContains,
        
        /// <summary>
        /// Starts with
        /// </summary>
        [JsonPropertyName("startsWith")]
        StartsWith,
        
        /// <summary>
        /// Ends with
        /// </summary>
        [JsonPropertyName("endsWith")]
        EndsWith,
        
        /// <summary>
        /// Is empty
        /// </summary>
        [JsonPropertyName("isEmpty")]
        IsEmpty,
        
        /// <summary>
        /// Is not empty
        /// </summary>
        [JsonPropertyName("isNotEmpty")]
        IsNotEmpty,
        
        /// <summary>
        /// Is in
        /// </summary>
        [JsonPropertyName("isIn")]
        IsIn,
        
        /// <summary>
        /// Is not in
        /// </summary>
        [JsonPropertyName("isNotIn")]
        IsNotIn
    }
    
    /// <summary>
    /// Represents a logical operator
    /// </summary>
    public enum TeableLogicalOperator
    {
        /// <summary>
        /// AND operator
        /// </summary>
        [JsonPropertyName("and")]
        And,
        
        /// <summary>
        /// OR operator
        /// </summary>
        [JsonPropertyName("or")]
        Or
    }
    
    /// <summary>
    /// Represents a filter condition
    /// </summary>
    public class TeableFilterCondition
    {
        /// <summary>
        /// The field to filter on
        /// </summary>
        [JsonPropertyName("fieldId")]
        public string FieldId { get; set; }
        
        /// <summary>
        /// The operator to use
        /// </summary>
        [JsonPropertyName("operator")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableFilterOperator Operator { get; set; }
        
        /// <summary>
        /// The value to filter by
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
    
    /// <summary>
    /// Represents a filter group
    /// </summary>
    public class TeableFilterGroup
    {
        /// <summary>
        /// The logical operator to use
        /// </summary>
        [JsonPropertyName("operator")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TeableLogicalOperator Operator { get; set; }
        
        /// <summary>
        /// The conditions in the group
        /// </summary>
        [JsonPropertyName("conditions")]
        public List<object> Conditions { get; set; } = new List<object>();
    }
    
    /// <summary>
    /// Represents a filter for Teable records
    /// </summary>
    public class TeableFilter
    {
        /// <summary>
        /// The root filter group
        /// </summary>
        private TeableFilterGroup _rootGroup;
        
        /// <summary>
        /// Creates a new filter with a single condition
        /// </summary>
        /// <param name="fieldId">The field ID to filter on</param>
        /// <param name="operator">The operator to use</param>
        /// <param name="value">The value to filter by</param>
        public TeableFilter(string fieldId, TeableFilterOperator @operator, object value)
        {
            _rootGroup = new TeableFilterGroup
            {
                Operator = TeableLogicalOperator.And,
                Conditions = new List<object>
                {
                    new TeableFilterCondition
                    {
                        FieldId = fieldId,
                        Operator = @operator,
                        Value = value
                    }
                }
            };
        }
        
        /// <summary>
        /// Creates a new filter with a logical operator
        /// </summary>
        /// <param name="logicalOperator">The logical operator to use</param>
        public TeableFilter(TeableLogicalOperator logicalOperator)
        {
            _rootGroup = new TeableFilterGroup
            {
                Operator = logicalOperator
            };
        }
        
        /// <summary>
        /// Adds a condition to the filter
        /// </summary>
        /// <param name="fieldId">The field ID to filter on</param>
        /// <param name="operator">The operator to use</param>
        /// <param name="value">The value to filter by</param>
        /// <returns>The filter</returns>
        public TeableFilter AddCondition(string fieldId, TeableFilterOperator @operator, object value)
        {
            _rootGroup.Conditions.Add(new TeableFilterCondition
            {
                FieldId = fieldId,
                Operator = @operator,
                Value = value
            });
            
            return this;
        }
        
        /// <summary>
        /// Adds a nested filter group to the filter
        /// </summary>
        /// <param name="logicalOperator">The logical operator to use</param>
        /// <returns>The nested filter group</returns>
        public TeableFilterGroup AddGroup(TeableLogicalOperator logicalOperator)
        {
            var group = new TeableFilterGroup
            {
                Operator = logicalOperator
            };
            
            _rootGroup.Conditions.Add(group);
            
            return group;
        }
        
        /// <summary>
        /// Converts the filter to a JSON string
        /// </summary>
        /// <returns>The JSON string</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(_rootGroup);
        }
        
        /// <summary>
        /// Converts the filter to a query string parameter
        /// </summary>
        /// <returns>The query string parameter</returns>
        public string ToQueryString()
        {
            return Uri.EscapeDataString(ToJson());
        }
    }
    
    /// <summary>
    /// Extension methods for TeableFilterGroup
    /// </summary>
    public static class TeableFilterGroupExtensions
    {
        /// <summary>
        /// Adds a condition to the filter group
        /// </summary>
        /// <param name="group">The filter group</param>
        /// <param name="fieldId">The field ID to filter on</param>
        /// <param name="operator">The operator to use</param>
        /// <param name="value">The value to filter by</param>
        /// <returns>The filter group</returns>
        public static TeableFilterGroup AddCondition(this TeableFilterGroup group, string fieldId, TeableFilterOperator @operator, object value)
        {
            group.Conditions.Add(new TeableFilterCondition
            {
                FieldId = fieldId,
                Operator = @operator,
                Value = value
            });
            
            return group;
        }
        
        /// <summary>
        /// Adds a nested filter group to the filter group
        /// </summary>
        /// <param name="group">The filter group</param>
        /// <param name="logicalOperator">The logical operator to use</param>
        /// <returns>The nested filter group</returns>
        public static TeableFilterGroup AddGroup(this TeableFilterGroup group, TeableLogicalOperator logicalOperator)
        {
            var nestedGroup = new TeableFilterGroup
            {
                Operator = logicalOperator
            };
            
            group.Conditions.Add(nestedGroup);
            
            return nestedGroup;
        }
    }
}
