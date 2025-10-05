<#
.SYNOPSIS
    Rotates the AES split key parts (p1–p4) in LocalInstallGuard.cs.

.DESCRIPTION
    Finds LocalInstallGuard.cs, generates 4 randomized segments, and replaces
    the existing p1–p4 definitions. Intended to be used before packaging a protected build.

    Example usage:
        ./Scripts/Rotate-InstallKey.ps1
        (or invoked automatically by Build-ProtectedRelease.ps1)
#>

[CmdletBinding()]
param(
    [string]$ProjectRoot = (Get-Location).Path
)

Write-Host "=== Rotating LocalInstallGuard AES key parts ===" -ForegroundColor Cyan

# ---- Locate the file ----
$guardFile = Get-ChildItem -Path $ProjectRoot -Recurse -Filter "LocalInstallGuard.cs" | Select-Object -First 1
if (-not $guardFile) {
    Write-Error "❌ Could not find LocalInstallGuard.cs!"
    exit 1
}
Write-Host "📄 Found: $($guardFile.FullName)"

# ---- Helper to create random part ----
function New-RandomPart {
    $chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
    -join ((1..3) | ForEach-Object { $chars[(Get-Random -Minimum 0 -Maximum $chars.Length)] })
}

# ---- Generate new key parts ----
$p1 = New-RandomPart
$p2 = New-RandomPart
$p3 = New-RandomPart
$p4 = New-RandomPart

Write-Host "🔑 New key parts:"
Write-Host "   p1 = $p1"
Write-Host "   p2 = $p2"
Write-Host "   p3 = $p3"
Write-Host "   p4 = $p4"

# ---- Read and modify file ----
$content = Get-Content $guardFile.FullName -Raw

# Replace old p1–p4 lines (they must follow format: string p1 = "XXX";)
$content = $content -replace 'string p1 = ".*?";', "string p1 = `"$p1`";"
$content = $content -replace 'string p2 = ".*?";', "string p2 = `"$p2`";"
$content = $content -replace 'string p3 = ".*?";', "string p3 = `"$p3`";"
$content = $content -replace 'string p4 = ".*?";', "string p4 = `"$p4`";"

# Save file
Set-Content -Path $guardFile.FullName -Value $content -Encoding UTF8
Write-Host "✅ LocalInstallGuard.cs updated successfully." -ForegroundColor Green

# ---- Optional: Show resulting combined key (for debug only) ----
$combined = "$p1$p2$p3$p4"
$base64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($combined))
Write-Host "🔒 Combined 16-char key (Base64 for verification only): $base64"

# ---- Optional: Git commit if working in repo ----
if (Test-Path ".git") {
    git add $guardFile.FullName | Out-Null
    git commit -m "Rotate AES install key parts ($((Get-Date).ToString('yyyy-MM-dd HH:mm:ss')))" | Out-Null
    Write-Host "📘 Git commit created for rotated AES key." -ForegroundColor Cyan
}

Write-Host "🎉 Key rotation complete." -ForegroundColor Green
