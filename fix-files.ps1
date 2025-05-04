$files = Get-ChildItem -Path "src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName
    $modified = $false
    $newContent = @()
    
    for ($i = 0; $i -lt $content.Length; $i++) {
        $line = $content[$i]
        
        # Check if this line contains "new Uri(" and the next line doesn't have a closing parenthesis
        if ($line -match "new Uri\(" -and $i -lt $content.Length - 1) {
            $nextLine = $content[$i + 1]
            if ($nextLine -match ";") {
                # Fix the missing closing parenthesis
                $line = $line -replace "new Uri\(", "new Uri("
                $nextLine = $nextLine -replace ";", ");"
                $newContent += $line
                $newContent += $nextLine
                $i++ # Skip the next line since we've already processed it
                $modified = $true
                continue
            }
        }
        
        $newContent += $line
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Fixed $($file.Name)"
    }
}
