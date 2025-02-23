#!/usr/bin/env pwsh
param(
    [Parameter(Position = 0, Mandatory)]
    [string]$Servername
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$certPemName = "./publish/tunnelgpt-cert.pem"
$certKeyName = "./publish/tunnelgpt-cert.key"


try {
    openssl req -x509 -newkey rsa:4096 -sha256 -nodes -days 36500 `
        -keyout $certKeyName `
        -out $certPemName `
        -subj "/C=NL/O=tolache/CN=$Servername" `
        -addext "subjectAltName=IP:$Servername"

    dos2unix $certKeyName $certPemName
}
catch {
    Write-Error "Failed to generate certificate. Reason:`n$_"
    exit 1
}