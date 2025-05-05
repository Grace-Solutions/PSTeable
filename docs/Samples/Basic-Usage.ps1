# Basic Usage of PSTeable Module
# This sample demonstrates the basic usage of the PSTeable module

# Import the module
Import-Module PSTeable

# Connect to Teable using a token
Connect-Teable -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"

# Get all spaces
$spaces = Get-TeableSpace
Write-Host "Found $($spaces.Count) spaces"

# Get all bases in the first space
$bases = Get-TeableBase -SpaceId $spaces[0].Id
Write-Host "Found $($bases.Count) bases in space $($spaces[0].Name)"

# Get all tables in the first base
$tables = Get-TeableTable -BaseId $bases[0].Id
Write-Host "Found $($tables.Count) tables in base $($bases[0].Name)"

# Get records from the first table (limit to 10)
$records = Get-TeableRecord -TableId $tables[0].Id -MaxCount 10
Write-Host "Retrieved $($records.Count) records from table $($tables[0].Name)"

# Display the records
$records | Format-Table -Property Id, @{Name="Fields"; Expression={$_.Fields | ConvertTo-Json -Compress}}

# Disconnect from Teable
Disconnect-Teable

