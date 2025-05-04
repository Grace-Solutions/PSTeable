# PSTeable Cmdlet Reference

This document provides a reference for all cmdlets in the PSTeable module.

## Authentication

### Connect-Teable

Connects to the Teable API.

```powershell
Connect-Teable -ApiKey <string> -BaseUrl <string> [-Verbose]
```

### Disconnect-Teable

Disconnects from the Teable API.

```powershell
Disconnect-Teable [-Verbose]
```

## Spaces

### Get-TeableSpace

Gets Teable spaces.

```powershell
Get-TeableSpace [[-SpaceId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

## Bases

### Get-TeableBase

Gets Teable bases.

```powershell
Get-TeableBase [[-SpaceId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Get-TeableBase [[-BaseId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### New-TeableBase

Creates a new Teable base.

```powershell
New-TeableBase -SpaceId <string> -Name <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Set-TeableBase

Updates a Teable base.

```powershell
Set-TeableBase -BaseId <string> -Name <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Remove-TeableBase

Removes a Teable base.

```powershell
Remove-TeableBase -BaseId <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Force] [-Verbose] [-WhatIf] [-Confirm]
```

## Tables

### Get-TeableTable

Gets Teable tables.

```powershell
Get-TeableTable [[-BaseId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Get-TeableTable [[-TableId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### New-TeableTable

Creates a new Teable table.

```powershell
New-TeableTable -BaseId <string> -Name <string> [-Description <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Set-TeableTable

Updates a Teable table.

```powershell
Set-TeableTable -TableId <string> [-Name <string>] [-Description <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Remove-TeableTable

Removes a Teable table.

```powershell
Remove-TeableTable -TableId <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Force] [-Verbose] [-WhatIf] [-Confirm]
```

## Fields

### Get-TeableField

Gets Teable fields.

```powershell
Get-TeableField [[-TableId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Get-TeableField [[-FieldId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### New-TeableField

Creates a new Teable field.

```powershell
New-TeableField -TableId <string> -Name <string> -Type <string> [-Options <Hashtable>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Set-TeableField

Updates a Teable field.

```powershell
Set-TeableField -FieldId <string> [-Name <string>] [-Options <Hashtable>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Remove-TeableField

Removes a Teable field.

```powershell
Remove-TeableField -FieldId <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Force] [-Verbose] [-WhatIf] [-Confirm]
```

## Records

### Get-TeableRecord

Gets Teable records.

```powershell
Get-TeableRecord -TableId <string> [[-RecordId] <string>] [-ViewId <string>] [-Filter <Hashtable>] [-SortBy <string[]>] [-Descending] [-Property <string[]>] [-MaxCount <int>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### New-TeableRecord

Creates a new Teable record.

```powershell
New-TeableRecord -TableId <string> -Fields <Hashtable> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
New-TeableRecord -TableId <string> -RecordPayload <TeableRecordPayload> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Set-TeableRecord

Updates a Teable record.

```powershell
Set-TeableRecord -TableId <string> -RecordId <string> -Fields <Hashtable> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Set-TeableRecord -TableId <string> -RecordId <string> -RecordPayload <TeableRecordPayload> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Remove-TeableRecord

Removes a Teable record.

```powershell
Remove-TeableRecord -TableId <string> -RecordId <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Force] [-Verbose] [-WhatIf] [-Confirm]
```

## Views

### Get-TeableView

Gets Teable views.

```powershell
Get-TeableView [[-TableId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Get-TeableView [[-ViewId] <string>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### New-TeableView

Creates a new Teable view.

```powershell
New-TeableView -TableId <string> -Name <string> -Type <string> [-Filter <Hashtable>] [-Sort <Hashtable>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Set-TeableView

Updates a Teable view.

```powershell
Set-TeableView -ViewId <string> [-Name <string>] [-Filter <Hashtable>] [-Sort <Hashtable>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Remove-TeableView

Removes a Teable view.

```powershell
Remove-TeableView -ViewId <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Force] [-Verbose] [-WhatIf] [-Confirm]
```

## Data Conversion & Sync

### ConvertTo-TeableRecord

Converts PowerShell objects to Teable records.

```powershell
ConvertTo-TeableRecord -InputObject <PSObject[]> [-FieldMapping <Hashtable>] [-Verbose]
```

### Export-TeableSchema

Exports the schema of a Teable base.

```powershell
Export-TeableSchema -BaseId <string> -Path <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Compare-TeableSchemas

Compares two Teable schemas.

```powershell
Compare-TeableSchemas -ReferenceSchemaPath <string> -DifferenceSchemaPath <string> [-Verbose]
```

### Import-TeableData

Imports data into a Teable table.

```powershell
Import-TeableData -TableId <string> -Path <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Export-TeableData

Exports data from a Teable table.

```powershell
Export-TeableData -TableId <string> -Path <string> [-Filter <string>] [-Fields <string[]>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```
