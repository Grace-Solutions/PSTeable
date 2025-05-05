using System;
using System.Management.Automation;
using PSTeable.Models;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Adds a sort specification to a Teable sort
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "TeableSort")]
    [OutputType(typeof(TeableSort))]
    public class AddTeableSort : PSCmdlet
    {
        /// <summary>
        /// The sort to add the specification to
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public TeableSort Sort { get; set; }
        
        /// <summary>
        /// The field ID to sort by
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string FieldId { get; set; }
        
        /// <summary>
        /// The sort direction
        /// </summary>
        [Parameter(Position = 2)]
        public TeableSortDirection Direction { get; set; } = TeableSortDirection.Ascending;
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Add the sort specification
                Sort.AddSort(FieldId, Direction);
                
                // Return the sort
                WriteObject(Sort);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "AddSortFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
