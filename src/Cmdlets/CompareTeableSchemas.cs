using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text.Json;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Compares two Teable schemas
    /// </summary>
    [Cmdlet(VerbsData.Compare, "TeableSchemas")]
    [OutputType(typeof(PSObject))]
    public class CompareTeableSchemas : PSCmdlet
    {
        /// <summary>
        /// The path to the first schema file
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string ReferenceSchemaPath { get; set; }
        
        /// <summary>
        /// The path to the second schema file
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string DifferenceSchemaPath { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Read the schemas from the files
                var referenceJson = File.ReadAllText(ReferenceSchemaPath);
                var differenceJson = File.ReadAllText(DifferenceSchemaPath);
                
                var referenceSchema = JsonSerializer.Deserialize<JsonElement>(referenceJson);
                var differenceSchema = JsonSerializer.Deserialize<JsonElement>(differenceJson);
                
                // Compare the schemas
                var differences = new List<PSObject>();
                
                // Compare base properties
                CompareProperty(differences, "Base.Id", GetPropertyValue(referenceSchema, "Id"), GetPropertyValue(differenceSchema, "Id"));
                CompareProperty(differences, "Base.Name", GetPropertyValue(referenceSchema, "Name"), GetPropertyValue(differenceSchema, "Name"));
                CompareProperty(differences, "Base.SpaceId", GetPropertyValue(referenceSchema, "SpaceId"), GetPropertyValue(differenceSchema, "SpaceId"));
                
                // Compare tables
                var referenceTables = GetPropertyValue(referenceSchema, "Tables");
                var differenceTables = GetPropertyValue(differenceSchema, "Tables");
                
                if (referenceTables.ValueKind == JsonValueKind.Array && differenceTables.ValueKind == JsonValueKind.Array)
                {
                    var referenceTableDict = new Dictionary<string, JsonElement>();
                    var differenceTableDict = new Dictionary<string, JsonElement>();
                    
                    // Build dictionaries of tables by ID
                    foreach (var table in referenceTables.EnumerateArray())
                    {
                        var id = GetPropertyValue(table, "Id").GetString();
                        if (!string.IsNullOrEmpty(id))
                        {
                            referenceTableDict[id] = table;
                        }
                    }
                    
                    foreach (var table in differenceTables.EnumerateArray())
                    {
                        var id = GetPropertyValue(table, "Id").GetString();
                        if (!string.IsNullOrEmpty(id))
                        {
                            differenceTableDict[id] = table;
                        }
                    }
                    
                    // Find tables that are in reference but not in difference
                    foreach (var id in referenceTableDict.Keys)
                    {
                        if (!differenceTableDict.ContainsKey(id))
                        {
                            var table = referenceTableDict[id];
                            var name = GetPropertyValue(table, "Name").GetString();
                            
                            differences.Add(new PSObject(new
                            {
                                Property = $"Table.{name}",
                                ReferenceValue = "Present",
                                DifferenceValue = "Missing"
                            }));
                        }
                    }
                    
                    // Find tables that are in difference but not in reference
                    foreach (var id in differenceTableDict.Keys)
                    {
                        if (!referenceTableDict.ContainsKey(id))
                        {
                            var table = differenceTableDict[id];
                            var name = GetPropertyValue(table, "Name").GetString();
                            
                            differences.Add(new PSObject(new
                            {
                                Property = $"Table.{name}",
                                ReferenceValue = "Missing",
                                DifferenceValue = "Present"
                            }));
                        }
                    }
                    
                    // Compare tables that are in both
                    foreach (var id in referenceTableDict.Keys)
                    {
                        if (differenceTableDict.ContainsKey(id))
                        {
                            var referenceTable = referenceTableDict[id];
                            var differenceTable = differenceTableDict[id];
                            
                            var referenceName = GetPropertyValue(referenceTable, "Name").GetString();
                            var differenceName = GetPropertyValue(differenceTable, "Name").GetString();
                            
                            if (referenceName != differenceName)
                            {
                                differences.Add(new PSObject(new
                                {
                                    Property = $"Table.{id}.Name",
                                    ReferenceValue = referenceName,
                                    DifferenceValue = differenceName
                                }));
                            }
                            
                            // Compare fields
                            var referenceFields = GetPropertyValue(referenceTable, "Fields");
                            var differenceFields = GetPropertyValue(differenceTable, "Fields");
                            
                            if (referenceFields.ValueKind == JsonValueKind.Array && differenceFields.ValueKind == JsonValueKind.Array)
                            {
                                var referenceFieldDict = new Dictionary<string, JsonElement>();
                                var differenceFieldDict = new Dictionary<string, JsonElement>();
                                
                                // Build dictionaries of fields by ID
                                foreach (var field in referenceFields.EnumerateArray())
                                {
                                    var fieldId = GetPropertyValue(field, "Id").GetString();
                                    if (!string.IsNullOrEmpty(fieldId))
                                    {
                                        referenceFieldDict[fieldId] = field;
                                    }
                                }
                                
                                foreach (var field in differenceFields.EnumerateArray())
                                {
                                    var fieldId = GetPropertyValue(field, "Id").GetString();
                                    if (!string.IsNullOrEmpty(fieldId))
                                    {
                                        differenceFieldDict[fieldId] = field;
                                    }
                                }
                                
                                // Find fields that are in reference but not in difference
                                foreach (var fieldId in referenceFieldDict.Keys)
                                {
                                    if (!differenceFieldDict.ContainsKey(fieldId))
                                    {
                                        var field = referenceFieldDict[fieldId];
                                        var fieldName = GetPropertyValue(field, "Name").GetString();
                                        
                                        differences.Add(new PSObject(new
                                        {
                                            Property = $"Table.{referenceName}.Field.{fieldName}",
                                            ReferenceValue = "Present",
                                            DifferenceValue = "Missing"
                                        }));
                                    }
                                }
                                
                                // Find fields that are in difference but not in reference
                                foreach (var fieldId in differenceFieldDict.Keys)
                                {
                                    if (!referenceFieldDict.ContainsKey(fieldId))
                                    {
                                        var field = differenceFieldDict[fieldId];
                                        var fieldName = GetPropertyValue(field, "Name").GetString();
                                        
                                        differences.Add(new PSObject(new
                                        {
                                            Property = $"Table.{referenceName}.Field.{fieldName}",
                                            ReferenceValue = "Missing",
                                            DifferenceValue = "Present"
                                        }));
                                    }
                                }
                                
                                // Compare fields that are in both
                                foreach (var fieldId in referenceFieldDict.Keys)
                                {
                                    if (differenceFieldDict.ContainsKey(fieldId))
                                    {
                                        var referenceField = referenceFieldDict[fieldId];
                                        var differenceField = differenceFieldDict[fieldId];
                                        
                                        var referenceFieldName = GetPropertyValue(referenceField, "Name").GetString();
                                        var differenceFieldName = GetPropertyValue(differenceField, "Name").GetString();
                                        
                                        if (referenceFieldName != differenceFieldName)
                                        {
                                            differences.Add(new PSObject(new
                                            {
                                                Property = $"Table.{referenceName}.Field.{fieldId}.Name",
                                                ReferenceValue = referenceFieldName,
                                                DifferenceValue = differenceFieldName
                                            }));
                                        }
                                        
                                        var referenceFieldType = GetPropertyValue(referenceField, "Type").GetString();
                                        var differenceFieldType = GetPropertyValue(differenceField, "Type").GetString();
                                        
                                        if (referenceFieldType != differenceFieldType)
                                        {
                                            differences.Add(new PSObject(new
                                            {
                                                Property = $"Table.{referenceName}.Field.{referenceFieldName}.Type",
                                                ReferenceValue = referenceFieldType,
                                                DifferenceValue = differenceFieldType
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Output the differences
                foreach (var difference in differences)
                {
                    WriteObject(difference);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CompareSchemasFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        private void CompareProperty(List<PSObject> differences, string propertyName, JsonElement referenceValue, JsonElement differenceValue)
        {
            if (referenceValue.ValueKind != differenceValue.ValueKind)
            {
                differences.Add(new PSObject(new
                {
                    Property = propertyName,
                    ReferenceValue = referenceValue.ToString(),
                    DifferenceValue = differenceValue.ToString()
                }));
                return;
            }
            
            switch (referenceValue.ValueKind)
            {
                case JsonValueKind.String:
                    if (referenceValue.GetString() != differenceValue.GetString())
                    {
                        differences.Add(new PSObject(new
                        {
                            Property = propertyName,
                            ReferenceValue = referenceValue.GetString(),
                            DifferenceValue = differenceValue.GetString()
                        }));
                    }
                    break;
                
                case JsonValueKind.Number:
                    if (referenceValue.GetDouble() != differenceValue.GetDouble())
                    {
                        differences.Add(new PSObject(new
                        {
                            Property = propertyName,
                            ReferenceValue = referenceValue.GetDouble(),
                            DifferenceValue = differenceValue.GetDouble()
                        }));
                    }
                    break;
                
                case JsonValueKind.True:
                case JsonValueKind.False:
                    if (referenceValue.GetBoolean() != differenceValue.GetBoolean())
                    {
                        differences.Add(new PSObject(new
                        {
                            Property = propertyName,
                            ReferenceValue = referenceValue.GetBoolean(),
                            DifferenceValue = differenceValue.GetBoolean()
                        }));
                    }
                    break;
            }
        }
        
        private JsonElement GetPropertyValue(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value))
            {
                return value;
            }
            
            return new JsonElement();
        }
    }
}

