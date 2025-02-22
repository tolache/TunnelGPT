#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$appsettingsFile = "./TunnelGPT/appsettings.json"
try {
    if (-not (Test-Path $appsettingsFile)) {
        Copy-Item ./TunnelGPT/appsettings.json.example $appsettingsFile
    }
    $json = Get-Content $appsettingsFile -Raw | ConvertFrom-Json
    $json.OPENAI_API_KEY      = $env:OPENAI_API_KEY
    $json.TELEGRAM_BOT_TOKEN  = $env:TELEGRAM_BOT_TOKEN
    $json.TELEGRAM_BOT_SECRET = $env:TELEGRAM_BOT_SECRET
    $json | ConvertTo-Json -Depth 10 | Set-Content $appsettingsFile -Encoding UTF8
}
catch {
    Write-Error "Failed to initialize appsettings.json. Reason:`n$_"
    exit 1
}