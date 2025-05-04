# Release Notes

## 1.1.0 - 2025-05-04

### Added
- Enhanced schema management:
  - Updated Export-TeableSchema to support exporting view schemas
  - Added Import-TeableSchema for importing base and view schemas
- Enhanced data export capabilities:
  - Added Export-TeableRecords with support for JSON, XML, and CSV formats
  - Added pipeline support for exporting records
  - Added CSV flattening support for nested objects and arrays

## 1.0.0 - 2025-05-03

### Added
- Initial release of PSTeable module
- Authentication cmdlets: Connect-Teable, Disconnect-Teable
- Space management: Get-TeableSpace
- Base management: Get-TeableBase, New-TeableBase, Set-TeableBase, Remove-TeableBase
- Table management: Get-TeableTable, New-TeableTable, Set-TeableTable, Remove-TeableTable
- Field management: Get-TeableField, New-TeableField, Set-TeableField, Remove-TeableField
- Record management: Get-TeableRecord, New-TeableRecord, Set-TeableRecord, Remove-TeableRecord
- View management: Get-TeableView, New-TeableView, Set-TeableView, Remove-TeableView
- Data conversion and sync: ConvertTo-TeableRecord, Export-TeableSchema, Compare-TeableSchemas, Import-TeableData, Export-TeableData
