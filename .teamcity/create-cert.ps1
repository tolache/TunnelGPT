#!/usr/bin/env pwsh
param(
    [Parameter(Position = 0, Mandatory)]
    [string]$Servername
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

openssl req -x509 -newkey rsa:4096 -sha256 -nodes -days 36500 `
    -keyout ./publish/tunnelgpt-cert.key `
    -out ./publish/tunnelgpt-cert.pem `
    -subj "/C=NL/O=tolache/CN=$Servername" `
    -addext "subjectAltName=IP:$Servername"