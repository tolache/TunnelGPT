# TunnelGPT Hosted

> [!NOTE]  
> This project is a **work in progress**.

This is the hosted alternative to the [AWS serverless](https://github.com/tolache/TunnelGPT/tree/aws-serverless) version of TunnelGPT.
It Can be run for free in Oracle Cloud (always free tier) and CockroachDB Cloud (free tier).

## Prerequisites

1. A paid OpenAI account. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
2. A registered Telegram Bot. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)
3. A server to run the bot on, such as an Oracle Cloud (always free tier) instance.
4. A Database server, such as CockroachDB Cloud (free tier).
5. Environment variables:
    - `OPENAI_API_KEY` set to a valid [OpenAI API token](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key).
    - `TELEGRAM_BOT_TOKEN` set to a valid [Telegram bot token](https://core.telegram.org/bots#how-do-i-create-a-bot).
    - `TELEGRAM_BOT_SECRET` set to a string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
      This secret verifies that requests to AWS originate from your Telegram webhook.