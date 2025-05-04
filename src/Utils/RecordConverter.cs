using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSTeable.Models;

namespace PSTeable.Utils
{
    /// <summary>
    /// Converts PowerShell objects to Teable records
    /// </summary>
    public static class RecordConverter
    {
        /// <summary>
        /// Converts a PowerShell object to a Teable record
        /// </summary>
        /// <param name="inputObject">The PowerShell object to convert</param>
        /// <param name="fieldMapping">Optional mapping of PowerShell property names to Teable field names</param>
        /// <returns>A Teable record payload</returns>
        public static TeableRecordPayload ConvertToRecord(PSObject inputObject, Hashtable fieldMapping = null)
        {
            var fields = new Dictionary<string, object>();
            
            foreach (var property in inputObject.Properties)
            {
                var fieldName = property.Name;
                
                // Apply field mapping if provided
                if (fieldMapping != null && fieldMapping.ContainsKey(fieldName))
                {
                    fieldName = fieldMapping[fieldName].ToString();
                }
                
                // Convert the property value to a Teable-compatible value
                var value = ConvertPropertyValue(property.Value);
                
                fields.Add(fieldName, value);
            }
            
            return new TeableRecordPayload { Fields = fields };
        }
        
        /// <summary>
        /// Converts a PowerShell property value to a Teable-compatible value
        /// </summary>
        /// <param name="value">The property value to convert</param>
        /// <returns>A Teable-compatible value</returns>
        private static object ConvertPropertyValue(object value)
        {
            if (value == null)
            {
                return null;
            }
            
            // Handle DateTime
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            
            // Handle arrays and lists
            if (value is IEnumerable enumerable && !(value is string))
            {
                var list = new List<object>();
                
                foreach (var item in enumerable)
                {
                    list.Add(ConvertPropertyValue(item));
                }
                
                return list;
            }
            
            // Handle PSObject
            if (value is PSObject psObject)
            {
                var dict = new Dictionary<string, object>();
                
                foreach (var property in psObject.Properties)
                {
                    dict.Add(property.Name, ConvertPropertyValue(property.Value));
                }
                
                return dict;
            }
            
            // Return primitive types as-is
            return value;
        }
        
        /// <summary>
        /// Determines the Teable field type for a .NET type
        /// </summary>
        /// <param name="type">The .NET type</param>
        /// <returns>The Teable field type</returns>
        public static string GetTeableFieldType(Type type)
        {
            if (type == typeof(string))
            {
                return "singleLineText";
            }
            
            if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float))
            {
                return "number";
            }
            
            if (type == typeof(bool))
            {
                return "checkbox";
            }
            
            if (type == typeof(DateTime))
            {
                return "dateTime";
            }
            
            // Default to single line text
            return "singleLineText";
        }
    }
}

