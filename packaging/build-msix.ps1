<#
.SYNOPSIS
  Phase 4C TaskTree MSIX packaging scaffold.

.DESCRIPTION
  Restores and publishes TaskTree.App as self-contained .NET 8 for Windows, then attempts to locate
  Windows MSIX packaging/signing tooling. This script intentionally avoids hardcoded secrets, cert passwords,
  production URLs, private keys, or token material.

  Architecture.md §12: MSIX + signtool.exe + self-contained .NET 8.
  Roadmap Phase 4C: build-msix.ps1 deliverable.
  Gaps #315-#332 documented in docs/signing-checklist.md and HANDOFF-v1.0.44-delta.md.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [ValidateSet('win-x64','win-arm64')]
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$CertificatePath,
    [securestring]$CertificatePassword,
    [string]$TimestampUrl = 'http://timestamp.digicert.com',
    [switch]$SkipSigning
)

$ErrorActionPreference = 'Stop'

function Write-Step([string]$Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

Write-Step "Validating repository root: $RepoRoot"
if (-not (Test-Path -LiteralPath (Join-Path $RepoRoot 'TaskTree.sln'))) {
    throw "TaskTree.sln not found. Run this script from a stitched repository root or pass -RepoRoot."
}

$appProject = Join-Path $RepoRoot 'src/TaskTree.App/TaskTree.App.csproj'
if (-not (Test-Path -LiteralPath $appProject)) {
    throw "TaskTree.App project not found at $appProject. Phase 5A/5B must reconcile project paths."
}

$publishDir = Join-Path $RepoRoot "artifacts/publish/$RuntimeIdentifier"
$msixOutDir = Join-Path $RepoRoot "artifacts/msix/$RuntimeIdentifier"
New-Item -ItemType Directory -Force -Path $publishDir, $msixOutDir | Out-Null

Write-Step "Restoring solution"
& dotnet restore (Join-Path $RepoRoot 'TaskTree.sln')

Write-Step "Publishing TaskTree.App self-contained for $RuntimeIdentifier"
& dotnet publish $appProject -c $Configuration -r $RuntimeIdentifier --self-contained true -o $publishDir

Write-Step "Checking for MakeAppx.exe"
$makeAppx = Get-Command MakeAppx.exe -ErrorAction SilentlyContinue
if (-not $makeAppx) {
    Write-Warning "MakeAppx.exe not found in PATH. Install Windows SDK/MSIX packaging tools. Packaging validation is deferred to Phase 5E."
    Write-Host "Published output is available at: $publishDir"
    exit 0
}

# This scaffold expects Codex/Claude Code to normalize the final MSIX layout once the stitched repo is available.
$packageName = "TaskTree-$RuntimeIdentifier.msix"
$packagePath = Join-Path $msixOutDir $packageName

Write-Step "Creating MSIX package scaffold with MakeAppx.exe"
& $makeAppx.Source pack /d $publishDir /p $packagePath /o

if ($SkipSigning) {
    Write-Warning "Skipping signing by request. Unsigned package path: $packagePath"
    exit 0
}

Write-Step "Checking for signtool.exe"
$signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
if (-not $signtool) {
    Write-Warning "signtool.exe not found. Install Windows SDK signing tools. Signing deferred to Phase 5E."
    Write-Host "Unsigned package path: $packagePath"
    exit 0
}

if (-not $CertificatePath) {
    Write-Warning "No -CertificatePath supplied. Signing skipped. Provide a code-signing certificate in Phase 5E."
    Write-Host "Unsigned package path: $packagePath"
    exit 0
}

if (-not (Test-Path -LiteralPath $CertificatePath)) {
    throw "Certificate path not found: $CertificatePath"
}

Write-Step "Signing MSIX package"
if ($CertificatePassword) {
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($CertificatePassword)
    try { $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
    & $signtool.Source sign /fd SHA256 /f $CertificatePath /p $plainPassword /tr $TimestampUrl /td SHA256 $packagePath
    $plainPassword = $null
} else {
    & $signtool.Source sign /fd SHA256 /f $CertificatePath /tr $TimestampUrl /td SHA256 $packagePath
}

Write-Step "MSIX package generated"
Write-Host $packagePath
