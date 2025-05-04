$files = @(
    "src/Cmdlets/GetTeableView.cs",
    "src/Cmdlets/NewTeableRecord.cs",
    "src/Cmdlets/NewTeableBase.cs",
    "src/Cmdlets/NewTeableTable.cs",
    "src/Cmdlets/RemoveTeableBase.cs",
    "src/Cmdlets/RemoveTeableField.cs",
    "src/Cmdlets/NewTeableField.cs",
    "src/Cmdlets/RemoveTeableView.cs",
    "src/Cmdlets/NewTeableView.cs",
    "src/Cmdlets/RemoveTeableTable.cs",
    "src/Cmdlets/RemoveTeableRecord.cs",
    "src/Cmdlets/ImportTeableData.cs"
)

foreach ($file in $files) {
    $content = Get-Content -Path $file -Raw
    $newContent = $content -replace 'new Uri\(([^)]+)\);', 'new Uri($1));'
    Set-Content -Path $file -Value $newContent
    Write-Host "Fixed $file"
}
