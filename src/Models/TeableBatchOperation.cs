using System;

namespace PSTeable.Models
{
    /// <summary>
    /// Represents the type of batch operation to perform
    /// </summary>
    public enum TeableBatchOperationType
    {
        /// <summary>
        /// Create new records
        /// </summary>
        Create,
        
        /// <summary>
        /// Update existing records
        /// </summary>
        Update,
        
        /// <summary>
        /// Delete records
        /// </summary>
        Delete
    }
    
    /// <summary>
    /// Represents the result of a batch operation
    /// </summary>
    public class TeableBatchResult
    {
        /// <summary>
        /// The type of operation that was performed
        /// </summary>
        public TeableBatchOperationType OperationType { get; set; }
        
        /// <summary>
        /// The number of records that were processed
        /// </summary>
        public int TotalRecords { get; set; }
        
        /// <summary>
        /// The number of records that were successfully processed
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// The number of records that failed to process
        /// </summary>
        public int FailureCount { get; set; }
        
        /// <summary>
        /// The time it took to process the batch
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Any errors that occurred during processing
        /// </summary>
        public string[] Errors { get; set; }
    }
}
