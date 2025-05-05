# PSTeable v2025.05.05.1041 Release

## Overview
PSTeable is a PowerShell module for interacting with the Teable API. This release includes enhanced documentation, improved project structure, and performance optimizations.

## What's New
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

## Fixed Issues
- Fixed duplicate verbose messages when using -Verbose
- Fixed issues with connection management
- Fixed error handling in HTTP requests
- Fixed path handling in export operations

## Installation
`powershell
# Install from PSGallery
Install-Module -Name PSTeable -RequiredVersion 2025.05.05.1041

# Or download and install manually
# 1. Download the ZIP file
# 2. Extract to a directory in your PSModulePath
# 3. Import the module
Import-Module PSTeable
`

## Documentation
For detailed documentation, see the [docs](./docs) directory.

## Sample Scripts
Check out the sample scripts in the [docs/Samples](./docs/Samples) directory to get started quickly.

## Requirements
- PowerShell 7.2 or later
- .NET Framework 4.7.2 or later

## License
MIT License
