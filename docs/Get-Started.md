# Getting Started with PSTeable

PSTeable is a PowerShell module that provides a convenient way to interact with the Teable REST API. This guide will help you get started with the module.

## Installation

```powershell
# Install from PowerShell Gallery
Install-Module -Name PSTeable -Scope CurrentUser

# Import the module
Import-Module PSTeable
```

## Authentication

Before you can use the module, you need to connect to the Teable API. You can connect directly or use connection profiles:

```powershell
# Connect directly
Connect-Teable -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"

# Or create and use connection profiles
New-TeableProfile -Name "Development" -Token "dev-api-token" -BaseUrl "https://dev.teable-instance.com/api"
Connect-Teable -ProfileName "Development"

# List available profiles
Get-TeableProfile

# Switch between profiles
Switch-TeableConnection -ProfileName "Production"

# Test your connection
Test-TeableConnection
```

## Working with Spaces

Spaces are the top-level containers in Teable. You can get all spaces or a specific space:

```powershell
# Get all spaces
Get-TeableSpace

# Get a specific space
Get-TeableSpace -SpaceId "space-id"
```

## Working with Bases

Bases are containers for tables. You can manage bases with the following commands:

```powershell
# Get all bases in a space
Get-TeableBase -SpaceId "space-id"

# Get a specific base
Get-TeableBase -BaseId "base-id"

# Create a new base
New-TeableBase -SpaceId "space-id" -Name "My New Base"

# Update a base
Set-TeableBase -BaseId "base-id" -Name "Updated Base Name"

# Remove a base
Remove-TeableBase -BaseId "base-id" -Confirm:$false
```

## Working with Tables

Tables contain records. You can manage tables with the following commands:

```powershell
# Get all tables in a base
Get-TeableTable -BaseId "base-id"

# Get a specific table
Get-TeableTable -TableId "table-id"

# Create a new table
New-TeableTable -BaseId "base-id" -Name "My New Table" -Description "Table description"

# Update a table
Set-TeableTable -TableId "table-id" -Name "Updated Table Name" -Description "Updated description"

# Remove a table
Remove-TeableTable -TableId "table-id" -Confirm:$false
```

## Working with Records

Records are the data in a table. You can manage records with the following commands:

```powershell
# Get all records in a table
Get-TeableRecord -TableId "table-id"

# Get records with advanced filtering
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"
Get-TeableRecord -TableId "table-id" -Filter $filter

# Get records with advanced sorting
$sort = New-TeableSort
$sort | Add-TeableSort -Field "CreatedTime" -Direction Descending
Get-TeableRecord -TableId "table-id" -Sort $sort

# Get records with projection
Get-TeableRecord -TableId "table-id" -Property "field-name1", "field-name2"

# Get a specific record
Get-TeableRecord -TableId "table-id" -RecordId "record-id"

# Create a new record
New-TeableRecord -TableId "table-id" -Fields @{
    "field-name1" = "value1"
    "field-name2" = "value2"
}

# Update a record
Set-TeableRecord -TableId "table-id" -RecordId "record-id" -Fields @{
    "field-name1" = "updated-value1"
    "field-name2" = "updated-value2"
}

# Remove a record
Remove-TeableRecord -TableId "table-id" -RecordId "record-id" -Confirm:$false
```

## Converting Objects to Records

You can convert PowerShell objects to Teable records:

```powershell
# Convert a PowerShell object to a Teable record
$object = [PSCustomObject]@{
    Name = "John Doe"
    Age = 30
    Email = "john.doe@example.com"
}

$record = $object | ConvertTo-TeableRecord

# Convert with field mapping
$record = $object | ConvertTo-TeableRecord -FieldMapping @{
    Name = "Full Name"
    Age = "Age (Years)"
    Email = "Email Address"
}
```

## Working with Fields

Fields define the structure of a table. You can manage fields with the following commands:

```powershell
# Get all fields in a table
Get-TeableField -TableId "table-id"

# Get a specific field
Get-TeableField -FieldId "field-id"

# Create a new field
New-TeableField -TableId "table-id" -Name "My Field" -Type "singleLineText"

# Create a select field with options
New-TeableField -TableId "table-id" -Name "Status" -Type "singleSelect" -Options @{
    "choices" = @(
        @{ "name" = "To Do"; "color" = "red" },
        @{ "name" = "In Progress"; "color" = "yellow" },
        @{ "name" = "Done"; "color" = "green" }
    )
}

# Update a field
Set-TeableField -FieldId "field-id" -Name "Updated Field Name"

# Remove a field
Remove-TeableField -FieldId "field-id" -Confirm:$false
```

## Working with Views

Views provide different ways to look at the data in a table. You can manage views with the following commands:

```powershell
# Get all views in a table
Get-TeableView -TableId "table-id"

# Get a specific view
Get-TeableView -ViewId "view-id"

# Create a new view
New-TeableView -TableId "table-id" -Name "My View" -Type "grid"

# Create a filtered view
New-TeableView -TableId "table-id" -Name "Filtered View" -Type "grid" -Filter @{
    "operator" = "and"
    "conditions" = @(
        @{
            "field" = "Status"
            "operator" = "="
            "value" = "Done"
        }
    )
}

# Update a view
Set-TeableView -ViewId "view-id" -Name "Updated View Name"

# Remove a view
Remove-TeableView -ViewId "view-id" -Confirm:$false
```

## Data Import and Export

You can import and export data with the following commands:

```powershell
# Export the schema of a base
Export-TeableSchema -BaseId "base-id" -Path "schema.json"

# Export the schema of a view
Export-TeableSchema -ViewId "view-id" -Path "view-schema.json"

# Compare two schemas
Compare-TeableSchemas -Path1 "schema1.json" -Path2 "schema2.json"

# Export data from a table to JSON
Export-TeableData -TableId "table-id" -Path "data.json" -Format Json

# Export data from a table to CSV with flattened nested objects
Export-TeableData -TableId "table-id" -Path "data.csv" -Format Csv -Flatten

# Export data from a table to XML
Export-TeableData -TableId "table-id" -Path "data.xml" -Format Xml

# Export filtered data
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"
Export-TeableData -TableId "table-id" -Path "active-records.json" -Filter $filter

# Import data into a table from JSON
Import-TeableData -TableId "table-id" -Path "data.json"

# Import data into a table from CSV
Import-TeableData -TableId "table-id" -Path "data.csv" -Format Csv

# Import data with field mapping
Import-TeableData -TableId "table-id" -Path "data.csv" -Format Csv -FieldMapping @{
    "Customer Name" = "Name"
    "Customer Email" = "Email"
}

# Import data with batch processing
Import-TeableData -TableId "table-id" -Path "large-data.json" -BatchSize 50 -ContinueOnError

# Update existing records using a key field
Import-TeableData -TableId "table-id" -Path "updated-data.json" -Update -KeyField "Email"
```

## Batch Operations

You can perform batch operations for improved performance:

```powershell
# Create multiple records in a single batch operation
$records = @(
    [PSCustomObject]@{
        Name = "John Doe"
        Email = "john.doe@example.com"
    },
    [PSCustomObject]@{
        Name = "Jane Smith"
        Email = "jane.smith@example.com"
    }
) | ConvertTo-TeableRecord

Invoke-TeableBatch -TableId "table-id" -Operation Create -Records $records

# Update multiple records in a single batch operation
$records = @(
    [PSCustomObject]@{
        Id = "record-id-1"
        Name = "John Doe Updated"
    },
    [PSCustomObject]@{
        Id = "record-id-2"
        Name = "Jane Smith Updated"
    }
) | ConvertTo-TeableRecord

Invoke-TeableBatch -TableId "table-id" -Operation Update -Records $records

# Delete multiple records in a single batch operation
Invoke-TeableBatch -TableId "table-id" -Operation Delete -RecordIds @("record-id-1", "record-id-2")
```

## Performance Measurement

You can measure the performance of operations:

```powershell
# Measure the performance of a script block
Measure-TeableOperation -ScriptBlock {
    Get-TeableRecord -TableId "table-id" -MaxCount 1000
} -Iterations 5 -WarmUp
```

## Disconnecting

When you're done, you can disconnect from the Teable API:

```powershell
Disconnect-Teable
```

## Additional Resources

For more examples, check out the sample scripts in the [Samples](./Samples) directory.


