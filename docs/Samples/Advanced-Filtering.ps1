# Advanced Filtering Sample
# This sample demonstrates how to use advanced filtering in PSTeable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -ProfileName "MyTeableInstance" # Replace with your profile name

# Create a simple filter for a single condition
$simpleFilter = New-TeableFilter
$simpleFilter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"

# Get records using the simple filter
$activeRecords = Get-TeableRecord -TableId "your-table-id" -Filter $simpleFilter
Write-Host "Found $($activeRecords.Count) active records"

# Create a complex filter with multiple conditions using AND
$complexFilter = New-TeableFilter -Operator And
$complexFilter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"
$complexFilter | Add-TeableFilterCondition -Field "Priority" -Operator "=" -Value "High"

# Get records using the complex filter
$highPriorityActiveRecords = Get-TeableRecord -TableId "your-table-id" -Filter $complexFilter
Write-Host "Found $($highPriorityActiveRecords.Count) high priority active records"

# Create a filter with nested groups using OR
$nestedFilter = New-TeableFilter -Operator Or

# Create a group for active high priority items
$activeHighGroup = $nestedFilter.AddGroup(And)
$activeHighGroup | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"
$activeHighGroup | Add-TeableFilterCondition -Field "Priority" -Operator "=" -Value "High"

# Create a group for completed critical items
$completedCriticalGroup = $nestedFilter.AddGroup(And)
$completedCriticalGroup | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Completed"
$completedCriticalGroup | Add-TeableFilterCondition -Field "Priority" -Operator "=" -Value "Critical"

# Get records using the nested filter
$filteredRecords = Get-TeableRecord -TableId "your-table-id" -Filter $nestedFilter
Write-Host "Found $($filteredRecords.Count) records matching the nested filter"

# Create a filter with comparison operators
$dateFilter = New-TeableFilter
$dateFilter | Add-TeableFilterCondition -Field "DueDate" -Operator "<=" -Value (Get-Date)

# Get records due today or earlier
$overdueRecords = Get-TeableRecord -TableId "your-table-id" -Filter $dateFilter
Write-Host "Found $($overdueRecords.Count) overdue records"

# Create a filter with contains operator
$searchFilter = New-TeableFilter
$searchFilter | Add-TeableFilterCondition -Field "Description" -Operator "contains" -Value "urgent"

# Get records with "urgent" in the description
$urgentRecords = Get-TeableRecord -TableId "your-table-id" -Filter $searchFilter
Write-Host "Found $($urgentRecords.Count) records with 'urgent' in the description"

# Disconnect from Teable
Disconnect-Teable

