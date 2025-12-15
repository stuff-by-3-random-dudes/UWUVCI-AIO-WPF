<#
.SYNOPSIS
  Obfuscate a GitHub PAT and inject it into GitHubBaseService.cs between marker comments.

.DESCRIPTION
  - Reads token from env var GITHUB_PAT or prompts interactively.
  - Generates a random xorKey (4 bytes by default).
  - Encodes the token as Base64, XORs the bytes, splits them into NUM_PARTS pieces.
  - Replaces the code in GitHubBaseService.cs between the markers:
      // BEGIN_TOKEN_REGION
      // END_TOKEN_REGION
    with a compiled C# snippet that reconstructs the token at runtime.
  - Creates a backup file GitHubBaseService.cs.bak by default.

.PARAMETER ProjectRoot
  Root path to search for GitHubBaseService.cs. Default: current directory.

.PARAMETER NumParts
  Number of parts to split the obfuscated bytes into. Default: 4.

.PARAMETER NoBackup
  Do not create a .bak backup (not recommended).

.PARAMETER NoRestore
  Do not attempt to restore original file on failure/exit (caller responsible).

.PARAMETER Restore
  If present, restore from .bak and exit.

.EXAMPLE
  pwsh ./Scripts/Inject-ObfuscatedToken.ps1
#>

[CmdletBinding()]
param(
    [string]$ProjectRoot = (Get-Location).Path,
    [int]$NumParts = 4,
    [switch]$NoBackup,
    [switch]$NoRestore,
    [switch]$Restore
)

function Write-Err([string]$m) { Write-Host "ERROR: $m" -ForegroundColor Red }
function Write-Ok([string]$m) { Write-Host "$m" -ForegroundColor Green }

# --- Locate target file ---
$target = Get-ChildItem -Path $ProjectRoot -Recurse -Filter "GitHubBaseService.cs" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $target) {
    Write-Err "Could not find GitHubBaseService.cs under '$ProjectRoot'."
    exit 2
}
$targetPath = $target.FullName
Write-Host "Target: $targetPath"

# --- Restore mode: restore .bak if present then exit ---
if ($Restore) {
    $bak = "$targetPath.bak"
    if (Test-Path $bak) {
        Move-Item -Force $bak $targetPath
        Write-Ok "Restored original file from: $bak"
        exit 0
    } else {
        Write-Err "Restore requested but backup not found: $bak"
        exit 3
    }
}

# --- Read token from env or prompt securely ---
$envToken = $env:GITHUB_PAT
if (![string]::IsNullOrWhiteSpace($envToken)) {
    $token = $envToken.Trim()
    Write-Host "Using GITHUB_PAT from environment."
} else {
    Write-Host "Enter GitHub PAT (ghp_... or gho_...). Input will be hidden."
    $secure = Read-Host -AsSecureString
    if (-not $secure) {
        Write-Err "No token provided. Aborting."
        exit 4
    }
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { $token = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
    $token = $token.Trim()
}

if ($token.Length -lt 10 -or (-not ($token.StartsWith("ghp_") -or $token.StartsWith("gho_")))) {
    Write-Err "Token validation failed. Token must start with 'ghp_' or 'gho_'."
    exit 5
}

# --- Prepare bytes: base64(token) -> bytes ---
$base64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($token))
$bytes = [System.Text.Encoding]::UTF8.GetBytes($base64)

# --- Generate xorKey (4 bytes by default) ---
$rand = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$xorLen = 4
$xorKey = New-Object 'System.Byte[]' ($xorLen)
$rand.GetBytes($xorKey)

# --- XOR the bytes in-place ---
for ($i = 0; $i -lt $bytes.Length; $i++) {
    $bytes[$i] = $bytes[$i] -bxor $xorKey[$i % $xorKey.Length]
}

# --- Split into parts ---
if ($NumParts -lt 1) { $NumParts = 4 }
$chunkSize = [math]::Ceiling($bytes.Length / $NumParts)
$parts = @()
for ($p = 0; $p -lt $NumParts; $p++) {
    $start = $p * $chunkSize
    $slice = New-Object 'System.Collections.Generic.List[byte]'
    for ($j = $start; ($j -lt $start + $chunkSize) -and ($j -lt $bytes.Length); $j++) { $slice.Add($bytes[$j]) }
    $parts += ,($slice.ToArray())
}

# --- Create backup ---
$bakPath = "$targetPath.bak"
if (-not $NoBackup) {
    try {
        Copy-Item -Path $targetPath -Destination $bakPath -Force
        Write-Host "Backup created: $bakPath"
    } catch {
        Write-Err "Failed to create backup: $_"
        if (-not $NoRestore) { Write-Host "No restore will be attempted."; }
        exit 6
    }
} else {
    Write-Host "Skipping backup as requested (NoBackup)."
}

# --- Build C# injection snippet ---
# produce xorKey literal
$xorLiteral = "new byte[] { " + (( $xorKey | ForEach-Object { "0x{0:X2}" -f $_ } ) -join ", ") + " }"


# produce int[] partN lines
$partLines = for ($i = 0; $i -lt $parts.Count; $i++) {
    $arr = $parts[$i]
    $values = if ($arr.Length -gt 0) { $arr -join ", " } else { "" }
    "int[] part$($i+1) = new int[] { $values };"
}

# produce parts-array initializer for byte[][] (convert int[] to byte[] at runtime)
$partsArrayInit = "var partsArray = new byte[][] { " + (
    ((1..$parts.Count) | ForEach-Object { "part$($_).Select(v => (byte)v).ToArray()" }) -join ", "
) + " };"

# final decode snippet (keeps markers)
$decodeSnippet = @"
    // BEGIN_TOKEN_REGION
    // This region is replaced by Inject-ObfuscatedToken.ps1 for protected builds.
    byte[] xorKey = $xorLiteral;
$(($partLines -join "`n    "))

    $partsArrayInit

    var all = partsArray.SelectMany(p => p)
                        .Select((b, idx) => (byte)(b ^ xorKey[idx % xorKey.Length]))
                        .ToArray();
    var base64Str = System.Text.Encoding.UTF8.GetString(all);
    var token = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
    return token;
    // END_TOKEN_REGION
"@

# --- Inject into file between markers ---
try {
    $content = Get-Content -Raw -Path $targetPath
    if ($content -notmatch "// BEGIN_TOKEN_REGION") {
        Write-Err "Target file does not contain marker '// BEGIN_TOKEN_REGION'. Aborting injection."
        if (-not $NoBackup -and (Test-Path $bakPath) -and -not $NoRestore) { Move-Item -Force $bakPath $targetPath }
        exit 7
    }

    # replace everything between the two markers (including them)
    $pattern = "(?s)// BEGIN_TOKEN_REGION.*?// END_TOKEN_REGION"
    $newContent = [regex]::Replace($content, $pattern, $decodeSnippet)

    Set-Content -Path $targetPath -Value $newContent -Encoding UTF8

    Write-Ok "Injected token snippet into $targetPath"
}
catch {
    Write-Err "Injection failed: $_"
    if (-not $NoBackup -and (Test-Path $bakPath) -and -not $NoRestore) {
        try { Move-Item -Force $bakPath $targetPath; Write-Host "Restored backup after failure." }
        catch { Write-Err "Restore also failed: $_" }
    }
    exit 8
}

# --- Output an audit hash of the obfuscated bytes (safe) ---
try {
    $sha = [System.BitConverter]::ToString((New-Object System.Security.Cryptography.SHA256Managed).ComputeHash($bytes)).Replace("-", "").ToLower()
    Write-Host "Obfuscated bytes SHA256: $sha"
} catch { }

Write-Ok "Done. Note: Backup is at: $bakPath (unless NoBackup was specified)."
if ($NoRestore) { Write-Warning "NoRestore specified: script will NOT attempt an automatic restore." }

exit 0
