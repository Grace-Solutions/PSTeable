# PSTeable

A full-featured, cross-platform PowerShell 7+ module that allows interaction with the [Teable REST API](https://help.teable.io/developer/api). PSTeable provides a comprehensive set of cmdlets for managing Teable spaces, bases, tables, fields, views, and records.

## Features

- Pure C# binary module (no .psm1)
- Works on Windows, macOS, and Linux
- PowerShell 7.2+ required
- Supports standard PowerShell verbose logging
- Full rate limit (429) handling and optional retry delay
- Fully supports pipeline input and tab completion
- Dynamic type mapping of PowerShell objects to Teable schema
- Centralized URL generation and API error handling
- Automatic pagination and object projection (`-Property`)
- Extensible schema conversion and bulk record upload utilities
- Multiple connection profiles for managing different Teable instances
- Advanced filtering, sorting, and search capabilities with IntelliSense
- Batch operations for improved performance
- CSV, JSON, and XML import/export with flattening support
- Secure credential storage for API tokens

## Installation

```powershell
# Install latest version from PowerShell Gallery
Install-Module -Name PSTeable -Scope CurrentUser

# Install specific version
Install-Module -Name PSTeable -RequiredVersion 2025.05.05.1339 -Scope CurrentUser

# Import the module
Import-Module PSTeable

# Check available commands
Get-Command -Module PSTeable
```

## Getting Started

```powershell
# Connect to Teable directly
Connect-Teable -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"

# Or create and use connection profiles
New-TeableProfile -Name "MyTeableInstance" -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"
Connect-Teable -ProfileName "MyTeableInstance"

# Get all spaces
Get-TeableSpace

# Get all bases in a space
Get-TeableBase -SpaceId "space-id"

# Get all tables in a base
Get-TeableTable -BaseId "base-id"

# Get records from a table
Get-TeableRecord -TableId "table-id" -MaxCount 100

# Use advanced filtering with IntelliSense
$filter = New-TeableFilter -FieldId "Status" -Operator Equal -Value "Active"
$filter | Add-TeableFilterCondition -FieldId "CreatedTime" -Operator GreaterThan -Value (Get-Date).AddDays(-7)
Get-TeableRecord -TableId "table-id" -Filter $filter

# Create complex filters with logical operators
$filter = New-TeableFilter -LogicalOperator And
$filter | Add-TeableFilterCondition -FieldId "Status" -Operator Equal -Value "Active"
$filter | Add-TeableFilterCondition -FieldId "Priority" -Operator GreaterThanOrEqual -Value 3
Get-TeableRecord -TableId "table-id" -Filter $filter

# Export records to CSV with flattening
Get-TeableRecord -TableId "table-id" | Export-TeableData -Path "records.csv" -Format CSV -Flatten

# Import data from CSV
Import-TeableData -TableId "table-id" -Path "data.csv" -Format CSV

# Export and import schema for backup or migration
Export-TeableSchema -BaseId "base-id" -Path "schema.json"
Import-TeableSchema -SpaceId "space-id" -Path "schema.json"
```

## Available Cmdlets

PSTeable provides the following cmdlet categories:

- **Authentication**: `Connect-Teable`, `Disconnect-Teable`
- **Spaces**: `Get-TeableSpace`
- **Bases**: `Get-TeableBase`, `New-TeableBase`, `Set-TeableBase`, `Remove-TeableBase`
- **Tables**: `Get-TeableTable`, `New-TeableTable`, `Set-TeableTable`, `Remove-TeableTable`
- **Fields**: `Get-TeableField`, `New-TeableField`, `Set-TeableField`, `Remove-TeableField`
- **Records**: `Get-TeableRecord`, `New-TeableRecord`, `Set-TeableRecord`, `Remove-TeableRecord`
- **Views**: `Get-TeableView`, `New-TeableView`, `Set-TeableView`, `Remove-TeableView`
- **Filters**: `New-TeableFilter`, `Add-TeableFilterCondition`
- **Data Conversion**: `Export-TeableSchema`, `Import-TeableSchema`, `Export-TeableData`, `Import-TeableData`

## Documentation

For detailed documentation, see the [docs](./docs) directory. The documentation includes:

- [Getting Started Guide](./docs/Getting-Started.md)
- [Cmdlet Reference](./docs/Cmdlet-Reference.md)
- [Sample Scripts](./docs/Samples/)
- [Release Notes](./docs/RELEASE-NOTES.md)

## Project Structure

- `src/` - Source code for the module and solution file
- `Module/` - Built module output
  - `Module/PSTeable/` - The PowerShell module
    - `Module/PSTeable/types/` - PowerShell type and format definitions
    - `Module/PSTeable/lib/` - Compiled .NET assemblies
- `docs/` - Documentation
  - `docs/Samples/` - Sample scripts demonstrating module usage
- `Scripts/` - Build and deployment scripts
- `Artifacts/` - Build output and release artifacts

## License

This project is licensed under the MIT License - see the LICENSE file for details.


