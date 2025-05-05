[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $SkipBuild
)

$ErrorActionPreference = 'Stop'

# Build the module if not skipped
if (-not $SkipBuild) {
    Write-Host "Building PSTeable module..."
    & "$PSScriptRoot\build.ps1" -Clean
}

# Create a timestamp for the release
$timestamp = Get-Date -Format "yyyy.MM.dd.HHmm"
$releasePath = "$PSScriptRoot\..\Releases\$timestamp"

# Create the release directory
Write-Host "Creating release directory: $releasePath"
New-Item -Path $releasePath -ItemType Directory -Force | Out-Null

# Copy the module to the release directory
Write-Host "Copying module to release directory..."
Copy-Item -Path "$PSScriptRoot\..\Module\PSTeable" -Destination $releasePath -Recurse -Force

Write-Host "Module published to: $releasePath\PSTeable" -ForegroundColor Green

# Optional: Create a ZIP file of the release
$zipPath = "$PSScriptRoot\..\Artifacts\PSTeable-$timestamp.zip"
Write-Host "Creating ZIP file: $zipPath"
New-Item -Path "$PSScriptRoot\..\Artifacts" -ItemType Directory -Force | Out-Null
Compress-Archive -Path "$releasePath\PSTeable" -DestinationPath $zipPath -Force

Write-Host "Module ZIP file created: $zipPath" -ForegroundColor Green
