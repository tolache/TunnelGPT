#!/usr/bin/env pwsh
param(
    [Parameter(Position = 0, Mandatory)]
    [string]$Servername
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$responseStatusCode = ""
$command = "curl -ks https://$Servername/ -w '%{http_code}' -o NUL"
try {
    $responseStatusCode = Invoke-Expression $command
}
catch {
    Write-Error "Failed to verify deployemnt using command '$command'. Reason:`n$_"
    exit 1
}

if ($responseStatusCode -ne 200) {
    Write-Error "Failed to verify deployemnt. Deployemnt status '$responseStatusCode' differrs from 200"
    exit 1
} else {
    Write-Ouput "Deployment verified successfully. 'https://$Servername/' returned status code 200."
}