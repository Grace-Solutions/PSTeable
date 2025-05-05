# Data Export Sample
# This sample demonstrates how to export data from Teable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"

# Get a table to export data from
$table = Get-TeableTable -BaseId "your-base-id" | Where-Object { $_.Name -eq "Customers" }

# Export all records to JSON
Export-TeableData -TableId $table.Id -Path "customers.json" -Format Json

# Export all records to CSV with flattened nested objects
Export-TeableData -TableId $table.Id -Path "customers.csv" -Format Csv -Flatten

# Export all records to XML
Export-TeableData -TableId $table.Id -Path "customers.xml" -Format Xml

# Create a filter to export only active customers
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"

# Export filtered records
Export-TeableData -TableId $table.Id -Path "active-customers.json" -Filter $filter

# Export only specific fields
Export-TeableData -TableId $table.Id -Path "customer-contacts.csv" -Format Csv -Fields @("Name", "Email", "Phone")

# Export from a view
$view = Get-TeableView -TableId $table.Id | Where-Object { $_.Name -eq "Active Customers" }
Export-TeableData -ViewId $view.Id -Path "active-customers-view.json"

# Pipeline example - get records and export them
Get-TeableRecord -TableId $table.Id -MaxCount 100 | 
    Export-TeableData -Path "first-100-customers.json"

# Disconnect from Teable
Disconnect-Teable

