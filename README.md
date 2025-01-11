# TunnelGPT

> [!NOTE]  
> This project is a **work in progress**.

This is a simple Telegram bot designed to bypass OpenAI's country restrictions by invoking OpenAI API through AWS Lambda.

## Run tests

```shell
dotnet test test/TunnelGPT.Tests
```

## Deploy

### Resources Created

The deployment will create the following resources in your AWS account (in the `eu-west-1` region, by default):

1. IAM role for running the Lambda function.
2. Lambda function that runs the main bot logic.
3. API Gateway for invoking the Lambda function with a request from a Telegram webhook.
4. Web Application Firewall for verifying the incoming requests are sent by the authorized Telegram webhook.

### Prerequisites

1. Tools:
   - AWS CLI (`aws --version`)
   - Terraform (`terraform --version`)
   - .NET Lambda tools (`dotnet lambda help`)
   - curl (`curl --version`)
2. Existing Lambda deployer IAM user (e.g., `lambda-deployer`) with:
   - Attached policies:
     - `AmazonAPIGatewayAdministrator`
     - `AmazonS3FullAccess`
     - `AWSLambda_FullAccess`
     - `AWSWAFFullAccess`
     - `IAMFullAccess`
3. AWS profile for the Lambda deployer user is configured in `~/.aws/credentials`:
   ```ini
   [lambda-deployer]
   aws_access_key_id = <your-access-key-id>
   aws_secret_access_key = <your-secret-access-key>
   ```
4. Environment variables:
   - `AWS_PROFILE` set to the Lambda deployer user profile (e.g., `lambda-profile`).
   - `OPENAI_API_KEY` set to a valid OpenAI API token. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
   - `TELEGRAM_BOT_TOKEN` set to a valid Telegram bot token. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)
   - `TELEGRAM_BOT_SECRET` set to a string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
        This secret verifies that requests to AWS originate from your Telegram webhook.  

### Deployment Steps

#### Linux/macOS

1. Initialize terraform.
   ```shell
   terraform -chdir=terraform init
   ```
2. Create IAM role.
   ```shell
   terraform -chdir=terraform apply \
       -var="telegram_bot_secret=$TELEGRAM_BOT_SECRET"
   ```
3. Deploy Lambda function.
   ```shell
   FUNCTION_ROLE=$(terraform -chdir=terraform output -raw lambda_role_name)    
   dotnet lambda deploy-function \
       --project-location src/TunnelGPT \
       --function-role $FUNCTION_ROLE \
       --environment-variables "OPENAI_API_KEY=$OPENAI_API_KEY;TELEGRAM_BOT_TOKEN=$TELEGRAM_BOT_TOKEN"
   ```
4. Deploy API Gateway and WAF.
   ```shell
   terraform -chdir=terraform apply \
       -var "telegram_bot_secret=$TELEGRAM_BOT_SECRET" \
       -var "lambda_is_deployed=true"
   ```
5. Set Telegram bot webhook.
   ```shell
   INVOKE_URL=$(terraform -chdir=terraform output -raw lambda_invoke_url)
   curl "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/setWebHook?url=$INVOKE_URL&secret_token=$TELEGRAM_BOT_SECRET"
   ```

#### Windows

1. Initialize terraform.
   ```powershell
   terraform -chdir=terraform init
   ```
2. Create IAM role.
   ```powershell
   terraform -chdir=terraform apply `
       -var="telegram_bot_secret=$env:TELEGRAM_BOT_SECRET"
   ```
3. Deploy Lambda function.
   ```powershell
   $function_role = terraform -chdir=terraform output -raw lambda_role_name
   dotnet lambda deploy-function `
       --project-location src/TunnelGPT `
       --function-role $function_role `
       --environment-variables "OPENAI_API_KEY=$env:OPENAI_API_KEY;TELEGRAM_BOT_TOKEN=$env:TELEGRAM_BOT_TOKEN"
   ```
4. Deploy API Gateway and WAF.
   ```powershell
   terraform -chdir=terraform apply `
       -var "telegram_bot_secret=$env:TELEGRAM_BOT_SECRET" `
       -var "lambda_is_deployed=true"
   ```
5. Set Telegram bot webhook.
   ```powershell
   $invoke_url = terraform -chdir=terraform output -raw lambda_invoke_url
   curl "https://api.telegram.org/bot$env:TELEGRAM_BOT_TOKEN/setWebHook?url=$invoke_url&secret_token=$env:TELEGRAM_BOT_SECRET"
   ```

## Remove

### Prerequisites

See [deployment prerequisites](#prerequisites).

### Removal Steps

#### Linux/macOS

1. Remove IAM role and API Gateway.
   ```shell
   terraform -chdir=terraform destroy -var="lambda_is_deployed=true" -var="telegram_bot_secret=$TELEGRAM_BOT_SECRET"
   ```
2. Remove Lambda.
   ```shell
   dotnet lambda delete-function --project-location src/TunnelGPT
   ```

#### Windows

1. Remove IAM role and API Gateway.
   ```powershell
   terraform -chdir=terraform destroy -var="lambda_is_deployed=true" -var="telegram_bot_secret=$env:TELEGRAM_BOT_SECRET"
   ```
2. Remove Lambda.
   ```powershell
   dotnet lambda delete-function --project-location src/TunnelGPT
   ```