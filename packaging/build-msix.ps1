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
    [string]$RepoRoot,
    [string]$DotnetPath,
    [string]$CertificatePath,
    [securestring]$CertificatePassword,
    [string]$TimestampUrl = 'http://timestamp.digicert.com',
    [switch]$SkipSigning
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

function Write-Step([string]$Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Resolve-DotnetPath {
    if ($DotnetPath) {
        if (-not (Test-Path -LiteralPath $DotnetPath -PathType Leaf)) {
            throw "The supplied .NET executable was not found: $DotnetPath"
        }
        return (Resolve-Path -LiteralPath $DotnetPath).Path
    }

    $repoDotnet = Join-Path $HOME '.dotnet/dotnet.exe'
    if (Test-Path -LiteralPath $repoDotnet -PathType Leaf) {
        return $repoDotnet
    }

    $command = Get-Command dotnet.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    throw 'dotnet.exe was not found. Install the .NET SDK or pass -DotnetPath.'
}

function Resolve-WindowsKitTool([string]$Name, [string]$Architecture) {
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $programFilesX86 = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
    $kitRoot = Join-Path $programFilesX86 'Windows Kits/10/bin'
    if (Test-Path -LiteralPath $kitRoot -PathType Container) {
        $candidate = Get-ChildItem -LiteralPath $kitRoot -Filter $Name -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "[\\/]$Architecture[\\/]" } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($candidate) { return $candidate.FullName }
    }

    return $null
}

Write-Step "Validating repository root: $RepoRoot"
if (-not (Test-Path -LiteralPath (Join-Path $RepoRoot 'TaskTree.sln'))) {
    throw "TaskTree.sln not found. Run this script from a stitched repository root or pass -RepoRoot."
}

$appProject = Join-Path $RepoRoot 'src/TaskTree.App/TaskTree.App.csproj'
if (-not (Test-Path -LiteralPath $appProject)) {
    throw "TaskTree.App project not found at $appProject. Phase 5A/5B must reconcile project paths."
}

$dotnet = Resolve-DotnetPath
Write-Step "Using .NET executable: $dotnet"

$manifestSource = Join-Path $RepoRoot 'packaging/Package.appxmanifest'
$assetSource = Join-Path $RepoRoot 'packaging/Assets'
$requiredAssets = @('StoreLogo.png', 'Square44x44Logo.png', 'Square150x150Logo.png', 'Wide310x150Logo.png')
if (-not (Test-Path -LiteralPath $manifestSource -PathType Leaf)) {
    throw "Package manifest not found: $manifestSource"
}
if (-not (Test-Path -LiteralPath $assetSource -PathType Container)) {
    throw "MSIX asset directory is missing: $assetSource. Owner-approved visual assets are required."
}
foreach ($asset in $requiredAssets) {
    $assetPath = Join-Path $assetSource $asset
    if (-not (Test-Path -LiteralPath $assetPath -PathType Leaf)) {
        throw "Required MSIX asset is missing: $assetPath"
    }
}

$publishDir = Join-Path $RepoRoot "artifacts/publish/$RuntimeIdentifier"
$msixOutDir = Join-Path $RepoRoot "artifacts/msix/$RuntimeIdentifier"
New-Item -ItemType Directory -Force -Path $publishDir, $msixOutDir | Out-Null

Write-Step "Restoring solution"
& $dotnet restore (Join-Path $RepoRoot 'TaskTree.sln')

Write-Step "Publishing TaskTree.App self-contained for $RuntimeIdentifier"
& $dotnet publish $appProject -c $Configuration -r $RuntimeIdentifier --self-contained true -o $publishDir

Write-Step "Checking for MakeAppx.exe"
$toolArchitecture = if ($RuntimeIdentifier -eq 'win-arm64') { 'arm64' } else { 'x64' }
$makeAppx = Resolve-WindowsKitTool 'MakeAppx.exe' $toolArchitecture
if (-not $makeAppx) {
    Write-Warning "MakeAppx.exe not found in PATH. Install Windows SDK/MSIX packaging tools. Packaging validation is deferred to Phase 5E."
    Write-Host "Published output is available at: $publishDir"
    exit 0
}

# MakeAppx requires the manifest and visual assets beside the published executable.
$packageLayoutDir = Join-Path $RepoRoot "artifacts/package-layout/$RuntimeIdentifier"
if (-not (Test-Path -LiteralPath (Join-Path $publishDir 'TaskTree.App.exe') -PathType Leaf)) {
    throw "Published executable not found: $(Join-Path $publishDir 'TaskTree.App.exe')"
}
New-Item -ItemType Directory -Force -Path $packageLayoutDir | Out-Null
Copy-Item -Path (Join-Path $publishDir '*') -Destination $packageLayoutDir -Recurse -Force
Copy-Item -LiteralPath $manifestSource -Destination (Join-Path $packageLayoutDir 'Package.appxmanifest') -Force
New-Item -ItemType Directory -Force -Path (Join-Path $packageLayoutDir 'Assets') | Out-Null
Copy-Item -Path (Join-Path $assetSource '*') -Destination (Join-Path $packageLayoutDir 'Assets') -Recurse -Force

$packageName = "TaskTree-$RuntimeIdentifier.msix"
$packagePath = Join-Path $msixOutDir $packageName

Write-Step "Creating MSIX package scaffold with MakeAppx.exe"
& $makeAppx pack /d $packageLayoutDir /p $packagePath /o

if ($SkipSigning) {
    Write-Warning "Skipping signing by request. Unsigned package path: $packagePath"
    exit 0
}

Write-Step "Checking for signtool.exe"
$signtool = Resolve-WindowsKitTool 'signtool.exe' $toolArchitecture
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
    & $signtool sign /fd SHA256 /f $CertificatePath /p $plainPassword /tr $TimestampUrl /td SHA256 $packagePath
    $plainPassword = $null
} else {
    & $signtool sign /fd SHA256 /f $CertificatePath /tr $TimestampUrl /td SHA256 $packagePath
}

Write-Step "MSIX package generated"
Write-Host $packagePath
