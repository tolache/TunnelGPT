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

A 100-year self-signed certificate example:

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

1. Upload the content of the `publish` directory to the target VM.
2. If you run the application as a non-root user on ports below 1024, ensure that `dotnet` binary has permission to bind to those ports:
    ```
    sudo setcap CAP_NET_BIND_SERVICE=+eip $(readlink -f /usr/bin/dotnet)
    ```
3. Ensure the firewall allows incoming connections to the application ports (`iptables` example for 80 and 443):
    ```shell
    for port in 80 443; do
      if iptables -C INPUT -m state --state NEW -p tcp --dport $port -j ACCEPT 2>/dev/null; then
        echo "A rule allowing port $port already exists. No changes made.";
      else
        echo "No rule found for port $port. Adding rule...";
        iptables -I INPUT 6 -m state --state NEW -p tcp --dport $port -j ACCEPT
        netfilter-persistent save
      fi
    done
    ```
4. Ensure ASP.NET Core Runtime 9.0 is installed. See [Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux).
5. Start the application:
    ```
    dotnet <application_home_dir>/TunnelGPT.dll
    ```
    For production deployment, you may want to register a service in `systemd`.