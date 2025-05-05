# Connection Profiles Sample
# This sample demonstrates how to use connection profiles in PSTeable

# Import the module
Import-Module PSTeable

# Create a new connection profile
New-TeableProfile -Name "Development" -Token "dev-api-token" -BaseUrl "https://dev.teable-instance.com/api"
New-TeableProfile -Name "Production" -Token "prod-api-token" -BaseUrl "https://prod.teable-instance.com/api"

# List all available profiles
Get-TeableProfile

# Connect using a profile
Connect-Teable -ProfileName "Development"

# Get all spaces in the development environment
$devSpaces = Get-TeableSpace
Write-Host "Found $($devSpaces.Count) spaces in development environment"

# Switch to the production environment
Switch-TeableConnection -ProfileName "Production"

# Get all spaces in the production environment
$prodSpaces = Get-TeableSpace
Write-Host "Found $($prodSpaces.Count) spaces in production environment"

# Disconnect from Teable
Disconnect-Teable

