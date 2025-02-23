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
6. A valid [OpenAI API token](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key). 
7. A valid [Telegram bot token](https://core.telegram.org/bots#how-do-i-create-a-bot).

## Build

### 1. Initialize appsettings*.json files

1. Copy the `appsettings*.json.example` files to `appsettings*.json` files.
2. Set the `OPENAI_API_KEY`, `TELEGRAM_BOT_TOKEN`, and `TELEGRAM_BOT_SECRET` to actual values either in the `appsettings*.json` files or via environment variables:
    - `OPENAI_API_KEY` - the OpenAI API that will be used by the application.
    - `TELEGRAM_BOT_TOKEN` - Telegram Bot token received from @BotFather.
    - `TELEGRAM_BOT_SECRET` - an arbitrary string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
      This secret verifies that requests to AWS originate from your Telegram webhook.

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
scp -i <path_to_ssh_key> publish/TunnelPGT.zip <deployer_user>@<target_servername>:/tmp/tunnelgpt
```

#### Windows

```pwsh
Compress-Archive -Path ./publish/* -DestinationPath ./publish/TunnelGPT.zip -Force
scp -i <path_to_ssh_key> publish/TunnelPGT.zip <deployer_user>@<target_servername>:/tmp/tunnelgpt
```

### 2. Install dependency packages on the target VM

Ensure these packages are installed on the deployment target machine:
- ASP.NET Core Runtime 9.0. See [Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux).
- Zip.

### 3. Set up the application

```shell
#!/bin/bash
set -euo pipefail

application_user="tunnelgpt"

# Initialize user
if ! id "$application_user" &>/dev/null; then
    useradd -m -s /bin/bash "$application_user"
fi

# Initialize application home
unzip /tmp/tunnelgpt/TunnelGPT.zip -d /opt/tunnelgpt
chown -R $application_user:$application_user /opt/tunnelgpt
chmod 600 /opt/tunnelgpt/appsettings*.json /opt/tunnelgpt/tunnelgpt-cert.*
rm /tmp/tunnelgpt/TunnelGPT.zip

# Allow dotnet to bind to well-known ports (required if environment is Production and application user is non-root)
setcap CAP_NET_BIND_SERVICE=+eip $(readlink -f /usr/bin/dotnet)

# Start the application
dotnet /opt/tunnelgpt/TunnelGPT.dll
```
