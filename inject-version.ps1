[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]
    $Version
)

$ErrorActionPreference = 'Stop'

# Update the version in the project file
$projectFile = ".\src\PSTeable.csproj"
$projectContent = Get-Content -Path $projectFile -Raw
$projectContent = $projectContent -replace '<Version>.*?</Version>', "<Version>$Version</Version>"
Set-Content -Path $projectFile -Value $projectContent

# Update the version in the module manifest
$manifestFile = ".\PSTeable.psd1"
$manifestContent = Get-Content -Path $manifestFile -Raw
$manifestContent = $manifestContent -replace "ModuleVersion = '.*?'", "ModuleVersion = '$Version'"
Set-Content -Path $manifestFile -Value $manifestContent

# Update the version in the VersionInfo.cs file
$versionInfoFile = ".\src\VersionInfo.cs"
$versionInfoContent = Get-Content -Path $versionInfoFile -Raw
$versionInfoContent = $versionInfoContent -replace 'AssemblyVersion\(".*?"\)', "AssemblyVersion(""$Version.0"")"
$versionInfoContent = $versionInfoContent -replace 'AssemblyFileVersion\(".*?"\)', "AssemblyFileVersion(""$Version.0"")"
Set-Content -Path $versionInfoFile -Value $versionInfoContent

Write-Host "Version updated to $Version" -ForegroundColor Green
