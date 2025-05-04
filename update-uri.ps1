$files = Get-ChildItem -Path "src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Replace HttpRequestMessage constructor with string URL to use Uri
    $newContent = $content -replace '(new HttpRequestMessage\(\s*[^,]+,\s*)([^)]+\))', '$1new Uri($2)'
    
    # Fix double Uri wrapping
    $newContent = $newContent -replace 'new Uri\(new Uri\(', 'new Uri('
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated $($file.Name)"
    }
}
