using System;
using System.Management.Automation;
using PSTeable.Models;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Creates a new Teable filter
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableFilter")]
    [OutputType(typeof(TeableFilter))]
    public class NewTeableFilter : PSCmdlet
    {
        /// <summary>
        /// The field ID to filter on
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Condition")]
        public string FieldId { get; set; }
        
        /// <summary>
        /// The operator to use
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Condition")]
        public TeableFilterOperator Operator { get; set; }
        
        /// <summary>
        /// The value to filter by
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "Condition")]
        public object Value { get; set; }
        
        /// <summary>
        /// The logical operator to use
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "LogicalOperator")]
        public TeableLogicalOperator LogicalOperator { get; set; }
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                TeableFilter filter;
                
                if (ParameterSetName == "Condition")
                {
                    // Create a filter with a single condition
                    filter = new TeableFilter(FieldId, Operator, Value);
                }
                else // LogicalOperator
                {
                    // Create a filter with a logical operator
                    filter = new TeableFilter(LogicalOperator);
                }
                
                WriteObject(filter);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateFilterFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
