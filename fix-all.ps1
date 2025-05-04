$files = Get-ChildItem -Path "src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Replace all HttpRequestMessage constructors with correct syntax
    $pattern = '(var\s+\w+\s*=\s*new\s+HttpRequestMessage\s*\(\s*[^,]+,\s*)(new\s+Uri\s*\(\s*[^)]+\)\s*);'
    $replacement = '$1$2);'
    
    $newContent = $content -replace $pattern, $replacement
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Fixed $($file.Name)"
    }
}
