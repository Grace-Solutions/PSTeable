# Performance Testing Sample
# This sample demonstrates how to measure performance of operations in PSTeable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -ProfileName "MyTeableInstance" # Replace with your profile name

# Define the table ID to use for testing
$tableId = "your-table-id" # Replace with your table ID

# Measure the performance of getting records
Write-Host "Measuring performance of Get-TeableRecord..."
$getRecordsResult = Measure-TeableOperation -ScriptBlock {
    Get-TeableRecord -TableId $tableId -MaxCount 100
} -Iterations 5 -WarmUp

Write-Host "Get-TeableRecord performance:"
Write-Host "Average duration: $($getRecordsResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "Minimum duration: $($getRecordsResult.MinimumDuration.TotalMilliseconds) ms"
Write-Host "Maximum duration: $($getRecordsResult.MaximumDuration.TotalMilliseconds) ms"
Write-Host "Standard deviation: $($getRecordsResult.StandardDeviation.TotalMilliseconds) ms"

# Measure the performance of filtering records
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"

Write-Host "Measuring performance of filtered Get-TeableRecord..."
$filteredGetResult = Measure-TeableOperation -ScriptBlock {
    Get-TeableRecord -TableId $tableId -Filter $filter -MaxCount 100
} -Iterations 5 -WarmUp

Write-Host "Filtered Get-TeableRecord performance:"
Write-Host "Average duration: $($filteredGetResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "Minimum duration: $($filteredGetResult.MinimumDuration.TotalMilliseconds) ms"
Write-Host "Maximum duration: $($filteredGetResult.MaximumDuration.TotalMilliseconds) ms"
Write-Host "Standard deviation: $($filteredGetResult.StandardDeviation.TotalMilliseconds) ms"

# Measure the performance of creating a record
$newRecord = [PSCustomObject]@{
    Name = "Performance Test"
    Email = "perf.test@example.com"
    Status = "Active"
} | ConvertTo-TeableRecord

Write-Host "Measuring performance of New-TeableRecord..."
$createRecordResult = Measure-TeableOperation -ScriptBlock {
    New-TeableRecord -TableId $tableId -Fields $newRecord.Fields
} -Iterations 3 -WarmUp

Write-Host "New-TeableRecord performance:"
Write-Host "Average duration: $($createRecordResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "Minimum duration: $($createRecordResult.MinimumDuration.TotalMilliseconds) ms"
Write-Host "Maximum duration: $($createRecordResult.MaximumDuration.TotalMilliseconds) ms"
Write-Host "Standard deviation: $($createRecordResult.StandardDeviation.TotalMilliseconds) ms"

# Measure the performance of batch operations
$batchRecords = @()
for ($i = 1; $i -le 10; $i++) {
    $batchRecords += [PSCustomObject]@{
        Name = "Batch Test $i"
        Email = "batch.test$i@example.com"
        Status = "Active"
    }
}
$batchRecords = $batchRecords | ConvertTo-TeableRecord

Write-Host "Measuring performance of Invoke-TeableBatch..."
$batchResult = Measure-TeableOperation -ScriptBlock {
    Invoke-TeableBatch -TableId $tableId -Operation Create -Records $batchRecords
} -Iterations 3 -WarmUp

Write-Host "Invoke-TeableBatch performance:"
Write-Host "Average duration: $($batchResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "Minimum duration: $($batchResult.MinimumDuration.TotalMilliseconds) ms"
Write-Host "Maximum duration: $($batchResult.MaximumDuration.TotalMilliseconds) ms"
Write-Host "Standard deviation: $($batchResult.StandardDeviation.TotalMilliseconds) ms"

# Measure the performance of exporting records
Write-Host "Measuring performance of Export-TeableData..."
$exportResult = Measure-TeableOperation -ScriptBlock {
    Export-TeableData -TableId $tableId -Format Json
} -Iterations 3 -WarmUp

Write-Host "Export-TeableData performance:"
Write-Host "Average duration: $($exportResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "Minimum duration: $($exportResult.MinimumDuration.TotalMilliseconds) ms"
Write-Host "Maximum duration: $($exportResult.MaximumDuration.TotalMilliseconds) ms"
Write-Host "Standard deviation: $($exportResult.StandardDeviation.TotalMilliseconds) ms"

# Compare performance of different export formats
Write-Host "Comparing performance of different export formats..."

Write-Host "Measuring JSON export performance..."
$jsonExportResult = Measure-TeableOperation -ScriptBlock {
    Export-TeableData -TableId $tableId -Format Json
} -Iterations 3 -WarmUp

Write-Host "Measuring CSV export performance..."
$csvExportResult = Measure-TeableOperation -ScriptBlock {
    Export-TeableData -TableId $tableId -Format Csv
} -Iterations 3 -WarmUp

Write-Host "Measuring XML export performance..."
$xmlExportResult = Measure-TeableOperation -ScriptBlock {
    Export-TeableData -TableId $tableId -Format Xml
} -Iterations 3 -WarmUp

Write-Host "Export format performance comparison:"
Write-Host "JSON: $($jsonExportResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "CSV: $($csvExportResult.AverageDuration.TotalMilliseconds) ms"
Write-Host "XML: $($xmlExportResult.AverageDuration.TotalMilliseconds) ms"

# Disconnect from Teable
Disconnect-Teable

