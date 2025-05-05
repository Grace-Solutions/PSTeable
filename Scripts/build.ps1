[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $Clean
)

$ErrorActionPreference = 'Stop'

# Clean the build artifacts if requested
if ($Clean) {
    Write-Host "Cleaning build artifacts..."
    if (Test-Path -Path "..\Module\PSTeable\lib") {
        Remove-Item -Path "..\Module\PSTeable\lib\*" -Recurse -Force
    }
    if (Test-Path -Path "..\Artifacts") {
        Remove-Item -Path "..\Artifacts\*" -Recurse -Force
    }
}

# Build the project
Write-Host "Building PSTeable module..."
Push-Location -Path "..\src"
try {
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Build failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

# The module manifest and type files are now maintained directly in the Module\PSTeable folder
Write-Host "Module manifest and type files are maintained in the Module\PSTeable folder"

Write-Host "Build completed!" -ForegroundColor Green
