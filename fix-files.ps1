$files = Get-ChildItem -Path "src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName
    $newContent = @()
    $inRequestBlock = $false
    $requestLines = @()
    
    foreach ($line in $content) {
        if ($line -match "var request = new HttpRequestMessage\(") {
            $inRequestBlock = $true
            $requestLines = @($line)
        }
        elseif ($inRequestBlock) {
            $requestLines += $line
            
            if ($line -match "\)\s*{" -or $line -match "\);" -or $line -match "^\s*\{") {
                $inRequestBlock = $false
                
                # Fix the request block
                $requestText = $requestLines -join "`n"
                
                # Replace with correct syntax
                $fixedText = $requestText -replace "new HttpRequestMessage\(\s*([^,]+),\s*([^)]+)\)\s*\{", "new HttpRequestMessage($1, new Uri($2)) {"
                $fixedText = $fixedText -replace "new HttpRequestMessage\(\s*([^,]+),\s*([^)]+)\);", "new HttpRequestMessage($1, new Uri($2));"
                
                # Add the fixed lines
                $newContent += $fixedText -split "`n"
            }
        }
        else {
            $newContent += $line
        }
    }
    
    Set-Content -Path $file.FullName -Value $newContent
    Write-Host "Fixed $($file.Name)"
}
