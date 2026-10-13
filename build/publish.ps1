#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Builds SnipDeck for distribution.

.DESCRIPTION
    Produces two output folders under build/output/:
      - framework-dependent/   small, requires .NET 10 Desktop Runtime
      - portable/              single-file self-contained .exe (no .NET required)

.PARAMETER Configuration
    Debug or Release. Defaults to Release.

.PARAMETER Runtime
    win-x64 or win-arm64. Defaults to win-x64.
#>

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path "$PSScriptRoot\.."
$appProject = Join-Path $root "src\SnipDeck.App\SnipDeck.App.csproj"
$outRoot = Join-Path $root "build\output"

# Clean previous output
if (Test-Path $outRoot) { Remove-Item -Recurse -Force $outRoot }
New-Item -ItemType Directory -Force -Path $outRoot | Out-Null

Write-Host "`n🔨 Publishing framework-dependent..." -ForegroundColor Cyan
dotnet publish $appProject `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o (Join-Path $outRoot "framework-dependent")

Write-Host "`n🔨 Publishing self-contained portable..." -ForegroundColor Cyan
dotnet publish $appProject `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o (Join-Path $outRoot "portable")

Write-Host "`n✅ Build complete." -ForegroundColor Green
Write-Host "   Framework-dependent: $(Join-Path $outRoot 'framework-dependent')"
Write-Host "   Portable:            $(Join-Path $outRoot 'portable')"
Write-Host ""
