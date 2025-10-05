<#
.SYNOPSIS
  Obfuscate a GitHub Personal Access Token and inject into GitHubCompatService.cs.

.DESCRIPTION
  - Reads token from env var GITHUB_PAT or prompts interactively.
  - Generates a random xorKey (N bytes).
  - Encodes token as Base64, XORs bytes, splits into NUM_PARTS parts.
  - Produces C# snippet and replaces the region between
      // BEGIN_TOKEN_REGION
      // END_TOKEN_REGION
    inside GitHubCompatService.cs.
  - Backs up original file to .bak. By default restores automatically after exit if -RestoreOriginal is specified.

.PARAMETER ProjectRoot
  Path of project root. Defaults to current dir.

.PARAMETER NumParts
  Number of split parts. Default 4.

.PARAMETER RestoreOriginal
  If present (default true), restore original file after exit. Use -NoRestore to keep the injected file.

.EXAMPLE
  pwsh ./Scripts/Inject-ObfuscatedToken.ps1
#>

[CmdletBinding()]
param(
    [string]$ProjectRoot = (Get-Location).Path,
    [int]$NumParts = 4,
    [switch]$NoRestore
)

function Prompt-For-Token {
    Write-Host "Enter GitHub PAT (ghp_... ). Will not echo:" -NoNewline
    $secure = Read-Host -AsSecureString
    if (-not $secure) { return $null }
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

# Find the target file
$target = Get-ChildItem -Path $ProjectRoot -Recurse -Filter "GitHubCompatService.cs" | Select-Object -First 1
if (-not $target) {
    Write-Error "Could not find GitHubCompatService.cs under $ProjectRoot"
    exit 1
}
$targetPath = $target.FullName
Write-Host "Target: $targetPath"

# Read token from env var or prompt
$envToken = $env:GITHUB_PAT
if (![string]::IsNullOrWhiteSpace($envToken)) {
    $token = $envToken.Trim()
    Write-Host "Using GITHUB_PAT from environment."
} else {
    $token = Prompt-For-Token
    if (-not $token) {
        Write-Error "No token supplied. Aborting."
        exit 1
    }
}

# Normalize token
$token = $token.Trim()

# Convert token -> base64 string -> bytes
$base64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($token))
$bytes = [System.Text.Encoding]::UTF8.GetBytes($base64)

# Generate a random xorKey (length 4..8; choose 4 for compactness)
$rand = New-Object System.Random
$xorLen = 4
$xorKey = (1..$xorLen | ForEach-Object { [byte]($rand.Next(0x10,0xF0)) })

# XOR bytes
for ($i=0; $i -lt $bytes.Length; $i++) {
    $bytes[$i] = $bytes[$i] -bxor $xorKey[$i % $xorKey.Length]
}

# Split into parts
$chunkSize = [math]::Ceiling($bytes.Length / $NumParts)
$parts = @()
for ($i=0; $i -lt $NumParts; $i++) {
    $start = $i * $chunkSize
    $slice = @()
    for ($j = $start; ($j -lt $start + $chunkSize) -and ($j -lt $bytes.Length); $j++) {
        $slice += $bytes[$j]
    }
    $parts += ,$slice
}

# Build C# snippet to inject
$xorKeyLiteral = "new byte[] { " + ($xorKey | ForEach-Object { "0x" + ($_).ToString("X2") } -join ", ") + " }"

$partsLiterals = for ($i=0; $i -lt $parts.Count; $i++) {
    $arr = $parts[$i]
    $elem = $arr -join ", "
    "int[] part$($i+1) = new int[] { $elem };"
}

# Decoding C# snippet
$decodeSnippet = @"
    // BEGIN_TOKEN_REGION
    byte[] xorKey = $xorKeyLiteral;
$(($partsLiterals -join "`n    "))
    var all = part1.Concat(part2).Concat(part3).Concat(part4)
                   .Select((x, idx) => (byte)(x ^ xorKey[idx % xorKey.Length]))
                   .ToArray();
    var base64Str = System.Text.Encoding.UTF8.GetString(all);
    var token = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
    return token;
    // END_TOKEN_REGION
"@

# NOTE: For general NumParts not equal 4, adjust the concat line programmatically.
if ($NumParts -ne 4) {
    # create dynamic concat
    $concatParts = (1..$NumParts | ForEach-Object { "part$_" }) -join ","
    $concatLine = "var all = (" + ($concatParts) + ").SelectMany(p => p).Select((x, idx) => (byte)(x ^ xorKey[idx % xorKey.Length])).ToArray();"
    # fallback: generate alternate snippet
    $decodeSnippet = @"
    // BEGIN_TOKEN_REGION
    byte[] xorKey = $xorKeyLiteral;
$(($partsLiterals -join "`n    "))
    // Dynamic concatenation for $NumParts parts
    var partsList = new System.Collections.Generic.List<byte[]>();
$(1..$NumParts | ForEach-Object { "    partsList.Add(part$($_).Select(v => (byte)v).ToArray());" } -join "`n")
    var allList = new System.Collections.Generic.List<byte>();
    foreach(var p in partsList) { allList.AddRange(p); }
    var all = allList.Select((x, idx) => (byte)(x ^ xorKey[idx % xorKey.Length])).ToArray();
    var base64Str = System.Text.Encoding.UTF8.GetString(all);
    var token = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
    return token;
    // END_TOKEN_REGION
"@
}

# Backup target
$bak = "$targetPath.bak"
Copy-Item -Path $targetPath -Destination $bak -Force
Write-Host "Backup created: $bak"

# Read file and replace region between BEGIN_TOKEN_REGION and END_TOKEN_REGION
$content = Get-Content $targetPath -Raw

if ($content -notmatch "// BEGIN_TOKEN_REGION") {
    Write-Error "Target file does not contain BEGIN_TOKEN_REGION marker. Aborting."
    exit 1
}

# Replace between markers
$pattern = "(?s)// BEGIN_TOKEN_REGION.*?// END_TOKEN_REGION"
$newContent = [regex]::Replace($content, $pattern, $decodeSnippet)

Set-Content -Path $targetPath -Value $newContent -Encoding UTF8
Write-Host "Injected obfuscated token into $targetPath"

if ($NoRestore) {
    Write-Warning "NoRestore specified: will NOT restore original file after this script exits."
    Write-Host "If you intend to leave token in file for packaging, ensure you do not commit it."
    exit 0
}

# By default restore after script exit? We expect caller (Build script) to remove restore if they want to keep token.
# We'll return 0 for success. Caller can decide whether to keep or restore.
Write-Host "Injection done. Caller should decide whether to keep or restore the file (default recommended: restore)."
exit 0
