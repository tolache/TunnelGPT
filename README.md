# TunnelGPT

This is a simple Telegram bot that forwards user requests to OpenAI's models,
enabling access in regions where ChatGPT is unavailable.

For a serverless deployment in AWS,
see the [aws-serverless](https://github.com/tolache/TunnelGPT/tree/aws-serverless) branch.

## Prerequisites

1. A paid OpenAI account. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
2. A registered Telegram Bot. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)
3. A Docker server to run the bot on.
4. .NET 9 for local development.

## Set up dev environment

### 1. Configure application settings

Set the variables below either in the `appsettings.json` file or via environment variables.

- `OPENAI_API_KEY` - the OpenAI API that will be used by the application.
- `TELEGRAM_BOT_TOKEN` - Telegram Bot token received from [@BotFather](https://telegram.me/BotFather).
- `TELEGRAM_BOT_SECRET` - an arbitrary string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
  This secret verifies that requests to AWS originate from your Telegram webhook.

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
docker buildx build . \
  --build-arg VERSION=2.0.0 \
  --build-arg REVISION=$(git rev-parse --short HEAD) \
  --platform linux/amd64,linux/arm64 \
  --tag tolache/tunnelgpt:latest \
  --load
docker buildx rm tunnelgpt
```

## Deploy

### 1. Start a TunnelGPT container

```shell
docker run -d --name tunnelgpt \
  -p 5000:80 \
  -e OPENAI_API_KEY="$OPENAI_API_KEY" \
  -e TELEGRAM_BOT_TOKEN="$TELEGRAM_BOT_TOKEN" \
  -e TELEGRAM_BOT_SECRET="$TELEGRAM_BOT_SECRET" \
  tolache/tunnelgpt:latest
 ```

### 2. Set up a Telegram bot webhook

If the webhook hasn't been set up, run:

```shell
curl "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/setWebHook?url=$TUNNELGPT_URL&secret_token=$TELEGRAM_BOT_SECRET"
```

`$TUNNELGPT_URL` - URL where the application is listening. Must be accessible from the internet.
