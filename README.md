# TunnelGPT Hosted

> [!NOTE]  
> This project is a **work in progress**.

This is the hosted alternative to the [AWS serverless](https://github.com/tolache/TunnelGPT/tree/aws-serverless) version of TunnelGPT,
a simple Telegram bot that forwards user requests to OpenAI's models,
enabling access in regions where ChatGPT is unavailable.

It can be run for free on Oracle Cloud (always free tier) and CockroachDB Cloud (free tier).

## Prerequisites

1. A paid OpenAI account. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
2. A registered Telegram Bot. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)
3. A server to run the bot on, such as an Oracle Cloud (always free tier) instance.
4. A Database server, such as CockroachDB Cloud (free tier).
5. .NET 9.
6. Environment variables:
    - `OPENAI_API_KEY` - a valid [OpenAI API token](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key).
    - `TELEGRAM_BOT_TOKEN` - a valid [Telegram bot token](https://core.telegram.org/bots#how-do-i-create-a-bot).
    - `TELEGRAM_BOT_SECRET` - an arbitrary string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
        This secret verifies that requests to AWS originate from your Telegram webhook.

## Build

### 1. Initialize appsettings*.json files

#### Option 1 (Manual)

1. Copy the `appsettings*.json.example` files to `appsettings*.json` files.
2. Set the `OPENAI_API_KEY`, `TELEGRAM_BOT_TOKEN`, and `TELEGRAM_BOT_SECRET` to actual values either in the `appsettings*.json` files or via environment variables.

#### Option 2 (Automatic)

This option requires the environment variables `OPENAI_API_KEY`, `TELEGRAM_BOT_TOKEN`, and `TELEGRAM_BOT_SECRET` to be set,
and it will only initialize `apssettings.json` (but not `appsettings.Development.json`).

##### Linux/macOS

```shell
#!/usr/bin/env bash
set -euo pipefail
appsettings_file="./TunnelGPT/appsettings.json"
if [ ! -f "$appsettings_file" ]; then
    cp "./TunnelGPT/appsettings.json.example" "$appsettings_file"
fi
jq --arg openai "$OPENAI_API_KEY" \
    --arg token "$TELEGRAM_BOT_TOKEN" \
    --arg secret "$TELEGRAM_BOT_SECRET" \
    '.OPENAI_API_KEY = $openai | .TELEGRAM_BOT_TOKEN = $token | .TELEGRAM_BOT_SECRET = $secret' \
    "$appsettings_file" > "${appsettings_file}.tmp" && mv "${appsettings_file}.tmp" "$appsettings_file"
```

##### Windows

```pwsh
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
```

### 2. Run tests

```shell
dotnet test
```

### 3. Build and publish the binaries

```shell
dotnet publish ./TunnelGPT -c Release -r linux-x64 --self-contained false -p:ContinuousIntegrationBuild=true -o ./publish
```

### 4. Create/copy certificate files

A TLS certificate is required for the server to listen on HTTPS.
The `appsettings.json` configuration expects PEM-formatted files named `tunnelgpt-cert.pem` and `tunnelgpt-cert.key`.
Place them in the `publish` directory after renaming.

To generate a self-signed certificate:

```shell
country="my-country"
organization="my-organization"
servername="my-servername"
openssl req -x509 -newkey rsa:4096 -sha256 -nodes -days 36500 \
  -keyout ./publish/tunnelgpt-cert.key \
  -out ./publish/tunnelgpt-cert.pem \
  -subj "/C=$country=/O=$organization/CN=$servername"
```

## Deploy

### 1. Upload the distro to the target VM

#### Linux/macOS

```shell
(cd publish && zip -r TunnelPGT.zip .)
scp -i <path_to_ssh_key> publish/TunnelPGT.zip <deployer_user>@<target_servername>:/tmp/
```

#### Windows

```pwsh
Compress-Archive -Path ./publish/* -DestinationPath ./publish/TunnelGPT.zip -Force
scp -i <path_to_ssh_key> publish/TunnelPGT.zip <deployer_user>@<target_servername>:/tmp/
```

### 2. Install dependency packages on the target VM

> [!WARNING]  
> This was tested on Ubuntu 24.04.
> Other distros/versions may behave differently.

```shell
#!/bin/bash
set -euo pipefail

export DEBIAN_FRONTEND=noninteractive

# Install zip
if which zip &>/dev/null; then
    echo "zip is already installed";
else
    sudo apt-get update
    sudo apt-get -y install zip
fi

# Install ASP.NET Core runtime
dotnet_runtime_package="aspnetcore-runtime-9.0"
if dpkg -s $dotnet_runtime_package &>/dev/null; then
    echo "$dotnet_runtime_package is already installed."
else
    sudo apt-get update
    sudo apt-get install -y software-properties-common
    sudo add-apt-repository -y ppa:dotnet/backports
    sudo apt-get install -y $dotnet_runtime_package
fi
```

### 3. Set up the application

```shell
#!/bin/bash
set -euo pipefail

application_user="tunnelgpt"
application_home="/opt/tunnelgpt"

# Uninstall application
if systemctl list-unit-files | grep '^tunnelgpt.service' &>/dev/null; then
    sudo systemctl stop tunnelgpt
fi
if [ -d $application_home ]; then
    sudo rm -rf $application_home
fi

# Initialize user
if ! id "$application_user" &>/dev/null; then
    sudo useradd -m -s /bin/bash "$application_user"
fi

# Initialize application home
sudo unzip /tmp/TunnelGPT.zip -d /opt/tunnelgpt
sudo chown -R $application_user:$application_user $application_home
sudo chmod 600 $application_home/appsettings*.json
rm /tmp/TunnelGPT.zip

# Register service
sudo tee /etc/systemd/system/tunnelgpt.service > /dev/null <<EOF
[Unit]
Description=TunnelGPT Telegram Bot
After=network.target

[Service]
Type=simple
User=$application_user
WorkingDirectory=$application_home
ExecStart=/usr/bin/dotnet $application_home/TunnelGPT.dll
Restart=always
RestartSec=30

[Install]
WantedBy=multi-user.target
EOF

# Start service
sudo systemctl daemon-reload
sudo systemctl enable tunnelgpt
sudo systemctl start tunnelgpt
```
