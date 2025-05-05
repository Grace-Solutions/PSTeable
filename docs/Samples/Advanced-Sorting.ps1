# Advanced Sorting Sample
# This sample demonstrates how to use advanced sorting in PSTeable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -ProfileName "MyTeableInstance" # Replace with your profile name

# Create a simple sort by a single field in ascending order
$simpleSort = New-TeableSort
$simpleSort | Add-TeableSort -Field "CreatedTime" -Direction Ascending

# Get records sorted by creation time (oldest first)
$oldestRecords = Get-TeableRecord -TableId "your-table-id" -Sort $simpleSort -MaxCount 10
Write-Host "Oldest 10 records:"
$oldestRecords | Format-Table -Property Id, @{Name="CreatedTime"; Expression={$_.CreatedTime}}

# Create a sort by a single field in descending order
$newestSort = New-TeableSort
$newestSort | Add-TeableSort -Field "CreatedTime" -Direction Descending

# Get records sorted by creation time (newest first)
$newestRecords = Get-TeableRecord -TableId "your-table-id" -Sort $newestSort -MaxCount 10
Write-Host "Newest 10 records:"
$newestRecords | Format-Table -Property Id, @{Name="CreatedTime"; Expression={$_.CreatedTime}}

# Create a multi-field sort (priority descending, then due date ascending)
$multiSort = New-TeableSort
$multiSort | Add-TeableSort -Field "Priority" -Direction Descending
$multiSort | Add-TeableSort -Field "DueDate" -Direction Ascending

# Get records sorted by priority (highest first) and then by due date (earliest first)
$prioritizedRecords = Get-TeableRecord -TableId "your-table-id" -Sort $multiSort
Write-Host "Records sorted by priority (highest first) and then by due date (earliest first):"
$prioritizedRecords | Format-Table -Property Id, @{Name="Priority"; Expression={$_.Fields["Priority"]}}, @{Name="DueDate"; Expression={$_.Fields["DueDate"]}}

# Combine filtering and sorting
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"

$sort = New-TeableSort
$sort | Add-TeableSort -Field "DueDate" -Direction Ascending

# Get active records sorted by due date (earliest first)
$activeRecordsByDueDate = Get-TeableRecord -TableId "your-table-id" -Filter $filter -Sort $sort
Write-Host "Active records sorted by due date (earliest first):"
$activeRecordsByDueDate | Format-Table -Property Id, @{Name="Status"; Expression={$_.Fields["Status"]}}, @{Name="DueDate"; Expression={$_.Fields["DueDate"]}}

# Disconnect from Teable
Disconnect-Teable

