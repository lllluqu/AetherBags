param(
    [string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$sln = Join-Path $root 'AetherBags.sln'
$dev = if ($env:DALAMUD_LIB_PATH) { $env:DALAMUD_LIB_PATH } else { Join-Path $env:APPDATA 'XIVLauncher\addon\Hooks\dev' }

if (-not (Test-Path $dev)) {
    throw "Dalamud dev libraries not found at '$dev'. Download https://goatcorp.github.io/dalamud-distrib/latest.zip and extract it there, or set DALAMUD_LIB_PATH."
}

Write-Host "Dalamud libs: $dev"
dotnet build $sln -c $Configuration -p:DalamudLibPath="$dev\"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$zip = Join-Path $root 'AetherBags\bin\x64\Release\AetherBagsCN\latest.zip'
if (Test-Path $zip) {
    Write-Host ""
    Write-Host "Package: $zip"
    Get-FileHash $zip -Algorithm SHA256 | Format-List
}
