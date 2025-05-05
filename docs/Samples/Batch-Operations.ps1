# Batch Operations Sample
# This sample demonstrates how to use batch operations in PSTeable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -ProfileName "MyTeableInstance" # Replace with your profile name

# Create a batch of records
$newRecords = @(
    [PSCustomObject]@{
        Name = "John Doe"
        Email = "john.doe@example.com"
        Status = "Active"
        Priority = "Medium"
    },
    [PSCustomObject]@{
        Name = "Jane Smith"
        Email = "jane.smith@example.com"
        Status = "Active"
        Priority = "High"
    },
    [PSCustomObject]@{
        Name = "Bob Johnson"
        Email = "bob.johnson@example.com"
        Status = "Inactive"
        Priority = "Low"
    }
) | ConvertTo-TeableRecord

# Create multiple records in a single batch operation
Write-Host "Creating $($newRecords.Count) records in a batch..."
$createdRecords = Invoke-TeableBatch -TableId "your-table-id" -Operation Create -Records $newRecords
Write-Host "Created $($createdRecords.Count) records"

# Update multiple records in a batch
$recordsToUpdate = $createdRecords | Where-Object { $_.Fields["Status"] -eq "Active" }
foreach ($record in $recordsToUpdate) {
    $record.Fields["Priority"] = "High"
}

Write-Host "Updating $($recordsToUpdate.Count) records in a batch..."
$updatedRecords = Invoke-TeableBatch -TableId "your-table-id" -Operation Update -Records $recordsToUpdate
Write-Host "Updated $($updatedRecords.Count) records"

# Delete multiple records in a batch
$recordsToDelete = $createdRecords | Where-Object { $_.Fields["Status"] -eq "Inactive" }
$recordIdsToDelete = $recordsToDelete | Select-Object -ExpandProperty Id

Write-Host "Deleting $($recordIdsToDelete.Count) records in a batch..."
Invoke-TeableBatch -TableId "your-table-id" -Operation Delete -RecordIds $recordIdsToDelete
Write-Host "Deleted $($recordIdsToDelete.Count) records"

# Create a large batch of records with batch size and delay
$largeRecordBatch = @()
for ($i = 1; $i -le 100; $i++) {
    $largeRecordBatch += [PSCustomObject]@{
        Name = "Test User $i"
        Email = "test.user$i@example.com"
        Status = if ($i % 2 -eq 0) { "Active" } else { "Inactive" }
        Priority = if ($i % 3 -eq 0) { "High" } elseif ($i % 3 -eq 1) { "Medium" } else { "Low" }
    }
}
$largeRecordBatch = $largeRecordBatch | ConvertTo-TeableRecord

Write-Host "Creating $($largeRecordBatch.Count) records in batches of 25 with 1 second delay between batches..."
$createdBatchRecords = Invoke-TeableBatch -TableId "your-table-id" -Operation Create -Records $largeRecordBatch -BatchSize 25 -BatchDelayMs 1000 -ContinueOnError
Write-Host "Created $($createdBatchRecords.Count) records"

# Measure the performance of batch operations vs. individual operations
Write-Host "Measuring performance of batch vs. individual operations..."

# Create 10 test records
$testRecords = @()
for ($i = 1; $i -le 10; $i++) {
    $testRecords += [PSCustomObject]@{
        Name = "Performance Test $i"
        Email = "perf.test$i@example.com"
        Status = "Active"
    }
}
$testRecords = $testRecords | ConvertTo-TeableRecord

# Measure batch operation
$batchResult = Measure-TeableOperation -ScriptBlock {
    Invoke-TeableBatch -TableId "your-table-id" -Operation Create -Records $testRecords
} -Iterations 1

Write-Host "Batch operation took $($batchResult.AverageDuration.TotalMilliseconds) ms"

# Measure individual operations
$individualResult = Measure-TeableOperation -ScriptBlock {
    foreach ($record in $testRecords) {
        New-TeableRecord -TableId "your-table-id" -Fields $record.Fields
    }
} -Iterations 1

Write-Host "Individual operations took $($individualResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "Performance improvement: $([Math]::Round(($individualResult.AverageDuration.TotalMilliseconds / $batchResult.AverageDuration.TotalMilliseconds), 2))x faster"

# Disconnect from Teable
Disconnect-Teable

