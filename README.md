# TunnelGPT

This is a simple Telegram bot that forwards user requests to OpenAI's models,
enabling access in regions where ChatGPT is unavailable.

For a serverless deployment in AWS,
see the [aws-serverless](https://github.com/tolache/TunnelGPT/tree/aws-serverless) branch.

## Prerequisites

1. A paid OpenAI account. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
2. A registered Telegram Bot. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)
3. A Docker or a Kubernetes cluster accessible from the internet to run the bot.
4. .NET 10 for local development.

## Set up dev environment

### 1. Configure application secrets

Set the values below via environment variables (recommended) or other [configuration providers](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers).

- `export OPENAI_API_KEY="your_key"` - the OpenAI API key that will be used.
- `export TELEGRAM_BOT_TOKEN="your_bot_token"` - Telegram Bot token received from [@BotFather](https://telegram.me/BotFather).
- `export TELEGRAM_WEBHOOK_SECRET="your_bot_secret"` - an arbitrary string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
  This secret verifies that requests to the bot originate from your Telegram webhook.

### 2. Run tests

```shell
dotnet test
```

### 3. Launch TunnelGPT from sources
 
```shell
dotnet run --project ./TunnelGPT
```

## Build

```shell
docker buildx create --name tunnelgpt --use
VERSION="3.0.1"
docker buildx build . \
  --build-arg VERSION=$VERSION \
  --build-arg REVISION=$(git rev-parse --short HEAD) \
  --platform linux/amd64,linux/arm64 \
  --tag tolache/tunnelgpt:$VERSION \
  --load
docker buildx rm tunnelgpt
```

## Deploy

### Option A: Docker

1. Start a TunnelGPT container

    ```shell
    docker run -d --name tunnelgpt \
      -p 5000:80 \
      -e OPENAI_API_KEY=$OPENAI_API_KEY \
      -e TELEGRAM_BOT_TOKEN=$TELEGRAM_BOT_TOKEN \
      -e TELEGRAM_WEBHOOK_SECRET=$TELEGRAM_WEBHOOK_SECRET \
      tolache/tunnelgpt:latest
    ```

2. Set up a Telegram bot webhook.

    ```shell
    curl "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/setWebHook?url=$TUNNELGPT_URL&secret_token=$TELEGRAM_WEBHOOK_SECRET"
    ```

   `$TUNNELGPT_URL` - URL where the application is listening. Must be accessible from the internet.

### Option B: Kubernetes

1. Run `cp charts/tunnelgpt/values.yaml charts/tunnelgpt/values.secrets.yaml` and populate the missing required values in the new file.

2. Install the Helm chart

    ```shell
    RELEASE_NAME="tunnelgpt-prod"
    NAMESPACE="tunnelgpt-prod"
    helm install $RELEASE_NAME charts/tunnelgpt -n $NAMESPACE --create-namespace --values charts/tunnelgpt/values.secrets.yaml
    ```

3. Follow the instructions from the NOTES displayed after the successful execution of the `helm install` command to set up a webhook.
