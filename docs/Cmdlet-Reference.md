# PSTeable Cmdlet Reference

This document provides a reference for all cmdlets in the PSTeable module.

## Authentication & Connection Management

### Connect-Teable

Connects to the Teable API.

```powershell
Connect-Teable -Token <SecureString> -BaseUrl <string> [-SaveProfile] [-ProfileName <string>] [-Verbose]
Connect-Teable -ProfileName <string> [-Verbose]
```

### Disconnect-Teable

Disconnects from the Teable API.

```powershell
Disconnect-Teable [-Verbose]
```

### Get-TeableConnection

Gets the current Teable connection.

```powershell
Get-TeableConnection [-Verbose]
```

### Get-TeableProfile

Gets Teable connection profiles.

```powershell
Get-TeableProfile [[-Name] <string>] [-Verbose]
```

### New-TeableProfile

Creates a new Teable connection profile.

```powershell
New-TeableProfile -Name <string> -Token <SecureString> -BaseUrl <string> [-Force] [-Verbose]
```

### Remove-TeableProfile

Removes a Teable connection profile.

```powershell
Remove-TeableProfile -Name <string> [-Force] [-Verbose] [-WhatIf] [-Confirm]
```

### Switch-TeableConnection

Switches to a different Teable connection profile.

```powershell
Switch-TeableConnection -ProfileName <string> [-Verbose]
```

### Test-TeableConnection

Tests the current Teable connection.

```powershell
Test-TeableConnection [-Verbose]
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
Get-TeableRecord -TableId <string> [[-RecordId] <string>] [-ViewId <string>] [-Filter <TeableFilter>] [-Sort <TeableSort>] [-Property <string[]>] [-MaxCount <int>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
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
New-TeableView -TableId <string> -Name <string> -Type <string> [-Filter <TeableFilter>] [-Sort <TeableSort>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Set-TeableView

Updates a Teable view.

```powershell
Set-TeableView -ViewId <string> [-Name <string>] [-Filter <TeableFilter>] [-Sort <TeableSort>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
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

### Update-TeableRecordDiff

Updates a Teable record with only the changed fields.

```powershell
Update-TeableRecordDiff -TableId <string> -RecordId <string> -OldRecord <TeableRecord> -NewRecord <TeableRecord> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Search-TeableRecords

Searches for records in a Teable table.

```powershell
Search-TeableRecords -TableId <string> -Query <string> [-MaxCount <int>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Invoke-TeableBatch

Performs batch operations on Teable records.

```powershell
Invoke-TeableBatch -TableId <string> -Operation <TeableBatchOperation> -Records <TeableRecord[]> [-BatchSize <int>] [-BatchDelayMs <int>] [-ContinueOnError] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Invoke-TeableBatch -TableId <string> -Operation Delete -RecordIds <string[]> [-BatchSize <int>] [-BatchDelayMs <int>] [-ContinueOnError] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Get-TeableChanges

Gets changes to records in a Teable table.

```powershell
Get-TeableChanges -TableId <string> -Since <DateTime> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Sync-TeableData

Synchronizes data between two Teable tables.

```powershell
Sync-TeableData -SourceTableId <string> -TargetTableId <string> [-FieldMapping <Hashtable>] [-KeyField <string>] [-Filter <TeableFilter>] [-DeleteMissing] [-BatchSize <int>] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Export-TeableSchema

Exports the schema of a Teable base or view.

```powershell
Export-TeableSchema -BaseId <string> -Path <string> [-IncludeViews] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Export-TeableSchema -ViewId <string> -Path <string> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Import-TeableSchema

Imports a schema to create or update a Teable base, table, or view.

```powershell
Import-TeableSchema -SpaceId <string> -Path <string> [-Name <string>] [-Force] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Import-TeableSchema -BaseId <string> -Path <string> [-Force] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Import-TeableSchema -TableId <string> -Path <string> [-Name <string>] [-Force] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Import-TeableSchema -ViewId <string> -Path <string> [-Force] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Compare-TeableSchemas

Compares two Teable schemas.

```powershell
Compare-TeableSchemas -Path1 <string> -Path2 <string> [-Verbose]
```

### Import-TeableData

Imports data into a Teable table.

```powershell
Import-TeableData -TableId <string> -Path <string> [-Format <TeableImportFormat>] [-Delimiter <string>] [-NoHeader] [-FieldMapping <Hashtable>] [-Update] [-KeyField <string>] [-BatchSize <int>] [-ContinueOnError] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Import-TeableData -TableId <string> -InputObject <PSObject[]> [-FieldMapping <Hashtable>] [-Update] [-KeyField <string>] [-BatchSize <int>] [-ContinueOnError] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Export-TeableData

Exports data from a Teable table or view.

```powershell
Export-TeableData -TableId <string> [-Path <string>] [-Format <TeableExportFormat>] [-Filter <TeableFilter>] [-Sort <TeableSort>] [-Fields <string[]>] [-Flatten] [-Delimiter <string>] [-NoHeader] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Export-TeableData -ViewId <string> [-Path <string>] [-Format <TeableExportFormat>] [-Filter <TeableFilter>] [-Sort <TeableSort>] [-Fields <string[]>] [-Flatten] [-Delimiter <string>] [-NoHeader] [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
Export-TeableData -Records <TeableRecord[]> [-Path <string>] [-Format <TeableExportFormat>] [-Fields <string[]>] [-Flatten] [-Delimiter <string>] [-NoHeader] [-Verbose]
```

## Filtering & Sorting

### New-TeableFilter

Creates a new Teable filter.

```powershell
New-TeableFilter [-Operator <TeableLogicalOperator>] [-Verbose]
```

### Add-TeableFilterCondition

Adds a condition to a Teable filter.

```powershell
Add-TeableFilterCondition -Filter <TeableFilter> -Field <string> -Operator <string> -Value <object> [-Verbose]
```

### New-TeableSort

Creates a new Teable sort.

```powershell
New-TeableSort [-Verbose]
```

### Add-TeableSort

Adds a sort field to a Teable sort.

```powershell
Add-TeableSort -Sort <TeableSort> -Field <string> [-Direction <TeableSortDirection>] [-Verbose]
```

## Reporting & Analytics

### Group-TeableData

Groups Teable records by a field.

```powershell
Group-TeableData -Records <TeableRecord[]> -GroupBy <string> [-Verbose]
```

### ConvertTo-TeableChart

Converts Teable records to a chart.

```powershell
ConvertTo-TeableChart -Records <TeableRecord[]> -ChartType <TeableChartType> -XField <string> -YField <string> [-GroupBy <string>] [-Verbose]
```

### New-TeableReport

Creates a new Teable report.

```powershell
New-TeableReport -TableId <string> -Name <string> -ReportType <TeableReportType> -Configuration <Hashtable> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

## Testing & Validation

### Test-TeableSchema

Tests a Teable schema for validity.

```powershell
Test-TeableSchema -Path <string> [-Verbose]
```

### Test-TeableData

Tests Teable data for validity.

```powershell
Test-TeableData -TableId <string> -Records <TeableRecord[]> [-RespectRateLimit] [-RateLimitDelay <TimeSpan>] [-Verbose]
```

### Measure-TeableOperation

Measures the performance of a Teable operation.

```powershell
Measure-TeableOperation -ScriptBlock <ScriptBlock> [-Iterations <int>] [-WarmUp] [-Verbose]
```


