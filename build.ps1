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
    if (Test-Path -Path ".\Module\PSTeable\lib") {
        Remove-Item -Path ".\Module\PSTeable\lib\*" -Recurse -Force
    }
}

# Build the project
Write-Host "Building PSTeable module..."
Push-Location -Path ".\src"
try {
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Build failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

# Copy the module manifest to the module directory
Write-Host "Copying module manifest..."
Copy-Item -Path ".\PSTeable.psd1" -Destination ".\Module\PSTeable\" -Force

# Copy the type and format files
Write-Host "Copying type and format files..."
if (!(Test-Path -Path ".\Module\PSTeable\types")) {
    New-Item -Path ".\Module\PSTeable\types" -ItemType Directory -Force | Out-Null
}
Copy-Item -Path ".\types\*.ps1xml" -Destination ".\Module\PSTeable\types\" -Force -ErrorAction SilentlyContinue

Write-Host "Build completed!" -ForegroundColor Green
