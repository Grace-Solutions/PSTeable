# Data Synchronization Sample
# This sample demonstrates how to synchronize data between tables in PSTeable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -ProfileName "MyTeableInstance" # Replace with your profile name

# Get source and target tables
$sourceTableId = "source-table-id" # Replace with your source table ID
$targetTableId = "target-table-id" # Replace with your target table ID

# Get the current record counts
$sourceRecords = Get-TeableRecord -TableId $sourceTableId
$targetRecords = Get-TeableRecord -TableId $targetTableId

Write-Host "Before synchronization:"
Write-Host "Source table: $($sourceRecords.Count) records"
Write-Host "Target table: $($targetRecords.Count) records"

# Define field mapping if field names are different between tables
$fieldMapping = @{
    "CustomerName" = "Name"
    "CustomerEmail" = "Email"
    "CustomerStatus" = "Status"
}

# Synchronize data from source to target
Write-Host "Synchronizing data from source to target..."
$syncResult = Sync-TeableData -SourceTableId $sourceTableId -TargetTableId $targetTableId -FieldMapping $fieldMapping -KeyField "Email" -BatchSize 50

# Display the synchronization results
Write-Host "Synchronization completed:"
Write-Host "Source records: $($syncResult.SourceRecords)"
Write-Host "Target records: $($syncResult.TargetRecords)"
Write-Host "Created: $($syncResult.Created)"
Write-Host "Updated: $($syncResult.Updated)"
Write-Host "Deleted: $($syncResult.Deleted)"
Write-Host "Unchanged: $($syncResult.Unchanged)"
Write-Host "Failed: $($syncResult.Failed)"
Write-Host "Duration: $($syncResult.Duration)"

# Get the updated record counts
$sourceRecords = Get-TeableRecord -TableId $sourceTableId
$targetRecords = Get-TeableRecord -TableId $targetTableId

Write-Host "After synchronization:"
Write-Host "Source table: $($sourceRecords.Count) records"
Write-Host "Target table: $($targetRecords.Count) records"

# Synchronize only active records
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"

Write-Host "Synchronizing only active records..."
$syncActiveResult = Sync-TeableData -SourceTableId $sourceTableId -TargetTableId $targetTableId -FieldMapping $fieldMapping -KeyField "Email" -Filter $filter -BatchSize 50

# Display the filtered synchronization results
Write-Host "Filtered synchronization completed:"
Write-Host "Source records: $($syncActiveResult.SourceRecords)"
Write-Host "Target records: $($syncActiveResult.TargetRecords)"
Write-Host "Created: $($syncActiveResult.Created)"
Write-Host "Updated: $($syncActiveResult.Updated)"
Write-Host "Deleted: $($syncActiveResult.Deleted)"
Write-Host "Unchanged: $($syncActiveResult.Unchanged)"
Write-Host "Failed: $($syncActiveResult.Failed)"
Write-Host "Duration: $($syncActiveResult.Duration)"

# Get changes since a specific date
$since = (Get-Date).AddDays(-7)
Write-Host "Getting changes since $since..."
$changes = Get-TeableChanges -TableId $sourceTableId -Since $since

Write-Host "Changes in the last 7 days:"
Write-Host "Created: $($changes.Created.Count) records"
Write-Host "Updated: $($changes.Updated.Count) records"
Write-Host "Deleted: $($changes.Deleted.Count) records"

# Disconnect from Teable
Disconnect-Teable

