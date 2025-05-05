using System;
using System.Management.Automation;
using PSTeable.Models;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Creates a new Teable sort
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableSort")]
    [OutputType(typeof(TeableSort))]
    public class NewTeableSort : PSCmdlet
    {
        /// <summary>
        /// The field ID to sort by
        /// </summary>
        [Parameter(Position = 0)]
        public string FieldId { get; set; }
        
        /// <summary>
        /// The sort direction
        /// </summary>
        [Parameter(Position = 1)]
        public TeableSortDirection Direction { get; set; } = TeableSortDirection.Ascending;
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                TeableSort sort;
                
                if (string.IsNullOrEmpty(FieldId))
                {
                    // Create an empty sort
                    sort = new TeableSort();
                }
                else
                {
                    // Create a sort with a single specification
                    sort = new TeableSort(FieldId, Direction);
                }
                
                WriteObject(sort);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateSortFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
