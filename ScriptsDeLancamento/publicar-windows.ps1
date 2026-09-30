$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$PublishDir = Join-Path $Root "artifacts\publish\win-x64"

if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null
Set-Location $Root

dotnet restore SmartGest.slnx
dotnet publish SmartGest.Desktop/SmartGest.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --property:PublishTrimmed=false --property:DebugType=None --output $PublishDir

Write-Host ""
Write-Host "Publicacao Windows criada em: $PublishDir"
Write-Host "No Windows, execute: iscc Installer\SmartGest.iss"
