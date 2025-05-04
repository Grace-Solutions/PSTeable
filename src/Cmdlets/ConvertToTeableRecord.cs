using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Converts PowerShell objects to Teable records
    /// </summary>
    [Cmdlet(VerbsData.Convert, "ToTeableRecord")]
    [OutputType(typeof(TeableRecordPayload))]
    public class ConvertToTeableRecord : PSCmdlet
    {
        /// <summary>
        /// The objects to convert
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public PSObject[] InputObject { get; set; }
        
        /// <summary>
        /// The mapping of PowerShell property names to Teable field names
        /// </summary>
        [Parameter()]
        public Hashtable FieldMapping { get; set; }
        
        /// <summary>
        /// The records to output
        /// </summary>
        private readonly List<TeableRecordPayload> _records = new List<TeableRecordPayload>();
        
        /// <summary>
        /// Processes each input object
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                foreach (var obj in InputObject)
                {
                    var record = RecordConverter.ConvertToRecord(obj, FieldMapping);
                    _records.Add(record);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ConversionFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Outputs the converted records
        /// </summary>
        protected override void EndProcessing()
        {
            foreach (var record in _records)
            {
                WriteObject(record);
            }
        }
    }
}
