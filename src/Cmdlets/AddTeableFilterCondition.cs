using System;
using System.Management.Automation;
using PSTeable.Models;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Adds a condition to a Teable filter
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "TeableFilterCondition")]
    [OutputType(typeof(TeableFilter))]
    public class AddTeableFilterCondition : PSCmdlet
    {
        /// <summary>
        /// The filter to add the condition to
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public TeableFilter Filter { get; set; }
        
        /// <summary>
        /// The field ID to filter on
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string FieldId { get; set; }
        
        /// <summary>
        /// The operator to use
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        public TeableFilterOperator Operator { get; set; }
        
        /// <summary>
        /// The value to filter by
        /// </summary>
        [Parameter(Mandatory = true, Position = 3)]
        public object Value { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Add the condition to the filter
                Filter.AddCondition(FieldId, Operator, Value);
                
                // Return the filter
                WriteObject(Filter);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "AddFilterConditionFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
