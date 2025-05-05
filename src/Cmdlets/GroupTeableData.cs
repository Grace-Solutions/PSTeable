using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSTeable.Models;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Groups Teable records by a field
    /// </summary>
    [Cmdlet(VerbsData.Group, "TeableData")]
    [OutputType(typeof(PSObject))]
    public class GroupTeableData : PSCmdlet
    {
        /// <summary>
        /// The records to group
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public TeableRecord[] Records { get; set; }
        
        /// <summary>
        /// The field to group by
        /// </summary>
        [Parameter(Mandatory = true)]
        public string GroupBy { get; set; }
        
        /// <summary>
        /// The fields to aggregate
        /// </summary>
        [Parameter()]
        public string[] AggregateFields { get; set; }
        
        /// <summary>
        /// The aggregation functions to use
        /// </summary>
        [Parameter()]
        public string[] AggregateFunctions { get; set; } = new[] { "Count" };
        
        /// <summary>
        /// The list of records to group
        /// </summary>
        private List<TeableRecord> _records = new List<TeableRecord>();
        
        /// <summary>
        /// Processes each record from the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Records != null)
            {
                _records.AddRange(Records);
            }
        }
        
        /// <summary>
        /// Processes the cmdlet after all pipeline input has been processed
        /// </summary>
        protected override void EndProcessing()
        {
            try
            {
                // Group the records by the specified field
                var groups = _records
                    .Where(r => r.Fields != null && r.Fields.ContainsKey(GroupBy))
                    .GroupBy(r => r.Fields[GroupBy]?.ToString() ?? "null")
                    .ToList();
                
                // Create the result objects
                foreach (var group in groups)
                {
                    var result = new PSObject();
                    result.Properties.Add(new PSNoteProperty("Group", group.Key));
                    result.Properties.Add(new PSNoteProperty("Count", group.Count()));
                    
                    // Add the records to the result
                    result.Properties.Add(new PSNoteProperty("Records", group.ToArray()));
                    
                    // Calculate aggregates
                    if (AggregateFields != null && AggregateFields.Length > 0)
                    {
                        foreach (var field in AggregateFields)
                        {
                            foreach (var function in AggregateFunctions)
                            {
                                var aggregateValue = CalculateAggregate(group, field, function);
                                result.Properties.Add(new PSNoteProperty($"{function}_{field}", aggregateValue));
                            }
                        }
                    }
                    
                    WriteObject(result);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "GroupDataFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Calculates an aggregate value for a group of records
        /// </summary>
        /// <param name="group">The group of records</param>
        /// <param name="field">The field to aggregate</param>
        /// <param name="function">The aggregation function</param>
        /// <returns>The aggregate value</returns>
        private object CalculateAggregate(IGrouping<string, TeableRecord> group, string field, string function)
        {
            // Get the values for the field
            var values = group
                .Where(r => r.Fields != null && r.Fields.ContainsKey(field))
                .Select(r => r.Fields[field])
                .Where(v => v != null)
                .ToList();
            
            // Calculate the aggregate value
            switch (function.ToLower())
            {
                case "count":
                    return values.Count;
                
                case "sum":
                    return values.Sum(v => Convert.ToDouble(v));
                
                case "average":
                case "avg":
                    return values.Average(v => Convert.ToDouble(v));
                
                case "min":
                    return values.Min(v => Convert.ToDouble(v));
                
                case "max":
                    return values.Max(v => Convert.ToDouble(v));
                
                case "first":
                    return values.FirstOrDefault();
                
                case "last":
                    return values.LastOrDefault();
                
                case "distinct":
                    return values.Distinct().Count();
                
                default:
                    throw new ArgumentException($"Unsupported aggregation function: {function}");
            }
        }
    }
}
