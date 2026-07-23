param(
    [string]$CssFilePath = $(throw "CssFilePath parameter is required"),
    [string]$IndexHtmlPath = $(throw "IndexHtmlPath parameter is required")
)

# Check if CSS file exists
if (-not (Test-Path $CssFilePath)) {
    Write-Error "CSS file not found at: $CssFilePath"
    exit 1
}

# Check if index.html exists
if (-not (Test-Path $IndexHtmlPath)) {
    Write-Error "index.html not found at: $IndexHtmlPath"
    exit 1
}

# Get the hash of the CSS file using .NET (more compatible)
try {
    $fileStream = [System.IO.File]::OpenRead($CssFilePath)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $hash = [System.BitConverter]::ToString($sha256.ComputeHash($fileStream)).Replace("-", "").Substring(0, 8)
    $fileStream.Close()
    $sha256.Dispose()
    Write-Host "Generated CSS hash: $hash"
} catch {
    Write-Error "Failed to generate hash: $_"
    exit 1
}

# Read the index.html file
try {
    $content = [System.IO.File]::ReadAllText($IndexHtmlPath)
} catch {
    Write-Error "Failed to read index.html: $_"
    exit 1
}

# Replace or add the hash to the CSS file reference
if ($content -match 'css/app\.min\.css\?v=[a-f0-9A-F]*') {
    # Replace existing hash (even if empty)
    $content = $content -replace 'css/app\.min\.css\?v=[a-f0-9A-F]*', "css/app.min.css?v=$hash"
    Write-Host "Updated existing hash in index.html"
} else {
    # Add new hash
    $content = $content -replace 'css/app\.min\.css', "css/app.min.css?v=$hash"
    Write-Host "Added new hash to index.html"
}

# Write back the index.html file
try {
    [System.IO.File]::WriteAllText($IndexHtmlPath, $content)
    Write-Host "Updated index.html successfully with hash: $hash"
    exit 0
} catch {
    Write-Error "Failed to write index.html: $_"
    exit 1
}
