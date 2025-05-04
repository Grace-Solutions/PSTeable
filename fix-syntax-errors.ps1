$files = Get-ChildItem -Path "src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Fix missing closing parenthesis in HttpRequestMessage constructor
    $newContent = $content -replace 'new Uri\(([^)]+)\);', 'new Uri($1));'
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Fixed $($file.Name)"
    }
}
