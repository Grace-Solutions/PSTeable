# PSTeable

A full-featured, cross-platform PowerShell 7+ binary C# module that allows interaction with the [Teable REST API](https://help.teable.io/developer/api).

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
- Advanced filtering, sorting, and search capabilities
- Batch operations for improved performance
- CSV and JSON import/export with flattening support

## Installation

```powershell
# Install from PowerShell Gallery
Install-Module -Name PSTeable -Scope CurrentUser

# Import the module
Import-Module PSTeable
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

# Use advanced filtering
$filter = New-TeableFilter
$filter | Add-TeableFilterCondition -Field "Status" -Operator "=" -Value "Active"
Get-TeableRecord -TableId "table-id" -Filter $filter

# Export records to CSV
Get-TeableRecord -TableId "table-id" | Export-TeableData -Path "records.csv" -Format Csv -Flatten
```

## Documentation

For detailed documentation, see the [docs](./docs) directory.

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


