# TunnelGPT

This is a simple Telegram bot designed to bypass OpenAI's country restrictions by invoking OpenAI API through AWS Lambda.

## Run tests

```shell
dotnet test test/TunnelGPT.Tests
```

## Deploy

### Resources Created

The deployment will create the following resources in your AWS account:

1. IAM role for running the Lambda function.
2. Lambda function.
3. API Gateway for accessing the Lambda function from Telegram.

### Prerequisites

1. Installed tools:
    - AWS CLI (`aws --version`)
    - Terraform (`terraform --version`)
    - .NET Lambda tools (`dotnet lambda help`)
2. IAM user `lambda-deployer` with:
    - Attached policies:
        - `AmazonAPIGatewayAdministrator`
        - `AmazonS3FullAccess`
        - `AWSLambda_FullAccess`
        - `IAMFullAccess`
    - Access key configured in `~/.aws/credentials`:
      ```ini
      [lambda-deployer]
      aws_access_key_id = <your-access-key-id>
      aws_secret_access_key = <your-secret-access-key>
      ```
3. Set environment variables:
   - `AWS_DEFAULT_PROFILE` set to `lambda-deployer`.
   - `OPENAI_API_KEY` is set to a valid OpenAI API token. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
   - `TELEGRAM_BOT_TOKEN` is set to a valid Telegram bot token. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)

### Deployment Steps

#### Linux/macOS

1. Initialize terraform.
    ```shell
    terraform -chdir=terraform init
    ```
2. Create IAM role.
    ```shell
    terraform -chdir=terraform apply
    ```
3. Deploy Lambda function.
    ```shell
    dotnet lambda deploy-function --project-location src/TunnelGPT --function-role TunnelGPT \
        --environment-variables "OPENAI_API_KEY=$OPENAI_API_KEY;TELEGRAM_BOT_TOKEN=$TELEGRAM_BOT_TOKEN"
    ```
4. Deploy API Gateway.
    ```shell
    terraform -chdir=terraform apply -var="lambda_is_deployed=true"
    ```

#### Windows

1. Initialize terraform.
    ```powershell
    terraform -chdir=terraform init
    ```
2. Create IAM role.
    ```powershell
    terraform -chdir=terraform apply
    ```
3. Deploy Lambda function.
    ```powershell
    dotnet lambda deploy-function --project-location src/TunnelGPT --function-role TunnelGPT `
        --environment-variables "OPENAI_API_KEY=$env:OPENAI_API_KEY;TELEGRAM_BOT_TOKEN=$env:TELEGRAM_BOT_TOKEN"
    ```
4. Deploy API Gateway.
    ```powershell
    terraform -chdir=terraform apply -var="lambda_is_deployed=true"
    ```
