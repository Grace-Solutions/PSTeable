$files = Get-ChildItem -Path "src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Fix missing closing parenthesis
    $newContent = $content -replace 'new HttpRequestMessage\(([^,]+), new Uri\(([^)]+)\);', 'new HttpRequestMessage($1, new Uri($2));'
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Fixed $($file.Name)"
    }
}
