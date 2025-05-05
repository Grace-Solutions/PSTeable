# Release Notes

## 2025.05.05.1041 - 2025-05-06

### Added
- Enhanced documentation and samples:
  - Added comprehensive sample scripts in docs/Samples
  - Updated all documentation to reflect latest features
  - Added detailed examples for all cmdlets
- Improved project structure:
  - Reorganized project to follow standard PowerShell module structure
  - Added Scripts folder for build and deployment scripts
  - Added Artifacts folder for build output
  - Moved types and module manifest to Module folder
- Performance improvements:
  - Optimized HTTP client usage with proper resource disposal
  - Improved error handling with specific exceptions
  - Enhanced verbose logging without custom timestamps
  - Added destination directory creation for export operations

### Fixed
- Fixed duplicate verbose messages when using -Verbose
- Fixed issues with connection management
- Fixed error handling in HTTP requests
- Fixed path handling in export operations

## 2025.05.04.1640 - 2025-05-05

### Added
- Multiple connection management:
  - Added profile-based connection management
  - Added Get-TeableProfile, New-TeableProfile, Remove-TeableProfile cmdlets
  - Added Switch-TeableConnection for switching between profiles
  - Added Get-TeableConnection for viewing active connections
  - Enhanced Test-TeableConnection with detailed diagnostics
- Advanced filtering and query support:
  - Added New-TeableFilter and Add-TeableFilterCondition for complex filters
  - Added New-TeableSort and Add-TeableSort for sorting records
  - Added Search-TeableRecords for full-text search
- Batch operations and performance:
  - Added Invoke-TeableBatch for bulk create/update/delete operations
  - Added Update-TeableRecordDiff for differential updates
  - Added Measure-TeableOperation for performance testing
- Synchronization and differential updates:
  - Added Get-TeableChanges for tracking record changes
  - Enhanced Sync-TeableData with bidirectional sync support
- Automation and workflow support:
  - Added webhook management cmdlets
  - Added automation management cmdlets
- Testing and validation tools:
  - Added Test-TeableSchema for schema validation
  - Added Test-TeableData for data validation
- Reporting and analytics:
  - Added Group-TeableData for data aggregation
  - Added ConvertTo-TeableChart for data visualization
  - Added New-TeableReport for generating reports

## 2025.05.04.1640 - 2025-05-04

### Added
- Enhanced schema management:
  - Updated Export-TeableSchema to support exporting view schemas
  - Added Import-TeableSchema for importing base and view schemas
- Enhanced data export capabilities:
  - Added Export-TeableData with support for JSON, XML, and CSV formats
  - Added pipeline support for exporting records
  - Added CSV flattening support for nested objects and arrays

## 2025.05.04.1640 - 2025-05-03

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




