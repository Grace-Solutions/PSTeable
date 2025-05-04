# PSTeable

A full-featured, cross-platform PowerShell 7+ binary C# module that allows interaction with the [Teable REST API](https://help.teable.io/developer/api).

## Features

- Pure C# binary module (no .psm1)
- Works on Windows, macOS, and Linux
- PowerShell 7.2+ required
- Supports structured logging via `-Verbose` with ISO timestamps
- Full rate limit (429) handling and optional retry delay
- Fully supports pipeline input and tab completion
- Dynamic type mapping of PowerShell objects to Teable schema
- Centralized URL generation and API error handling
- Automatic pagination and object projection (`-Property`)
- Extensible schema conversion and bulk record upload utilities

## Installation

```powershell
# Install from PowerShell Gallery
Install-Module -Name PSTeable -Scope CurrentUser

# Import the module
Import-Module PSTeable
```

## Getting Started

```powershell
# Connect to Teable
Connect-Teable -ApiKey "your-api-key" -BaseUrl "https://your-teable-instance.com/api"

# Get all spaces
Get-TeableSpace

# Get all bases in a space
Get-TeableBase -SpaceId "space-id"

# Get all tables in a base
Get-TeableTable -BaseId "base-id"

# Get records from a table
Get-TeableRecord -TableId "table-id" -MaxCount 100
```

## Documentation

For detailed documentation, see the [docs](./docs) directory.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
