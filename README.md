# TunnelGPT Serverless

This is a simple Telegram bot that forwards user requests to OpenAI's models, enabling access in regions where ChatGPT is unavailable.

This branch deploys as a serverless application on AWS, which requires **a paid component**: AWS WAF with a WebACL.  
For a hosted alternative, see the [hosted](https://github.com/tolache/TunnelGPT/tree/hosted) branch.

## Test

### Tester Requirements

Tools required on the machine running tests:

1. .NET 8 or newer (`dotnet --list-sdks`)
2. Docker (`docker --version`)

### Run Tests

```shell
dotnet test
```

## Deploy

### Resources Created

The deployment will create the following resources in your AWS account:

1. IAM role for running the Lambda function.
2. Lambda function that runs the main bot logic.
3. A CloudWatch log group.
4. API Gateway for invoking the Lambda function with a request from a Telegram webhook.
5. WAF with an ACL for verifying the incoming requests are sent by the authorized Telegram webhook. This is a paid service.

### Deployer Requirements

1. Tools:
   - .NET 8 (`dotnet --list-sdks`)
   - AWS CLI (`aws --version`)
   - Terraform (`terraform --version`)
   - curl (`curl --version`)
2. Existing Lambda deployer IAM user (e.g., `lambda-deployer`) with enough permissions to create the 
   [resources it needs to create](#resources-created). As an example, that can be achieved by attaching the following 
   policies:
     - `AmazonAPIGatewayAdministrator`
     - `AmazonDynamoDBFullAccess`
     - `AmazonS3FullAccess`
     - `AWSLambda_FullAccess`
     - `AWSWAFFullAccess`
     - `CloudWatchLogsFullAccess`
     - `IAMFullAccess`
3. A configured Lambda deployer AWS profile with valid credentials and a set region. Example config:
   1. `~/.aws/credentials`:
      ```ini
      [lambda-deployer]
      aws_access_key_id = <your-access-key-id>
      aws_secret_access_key = <your-secret-access-key>
      ```
   2. `~/.aws/config`:
      ```ini
      [profile lambda-deployer]
      region = eu-west-1
      output = json
      ```
   3. `AWS_PROFILE` set to `lambda-deployer`
4. Environment variables:
   - `OPENAI_API_KEY` set to a valid OpenAI API token. [Where do I find my OpenAI API Key?](https://help.openai.com/articles/4936850-where-do-i-find-my-openai-api-key)
   - `TELEGRAM_BOT_TOKEN` set to a valid Telegram bot token. [How Do I Create a Bot?](https://core.telegram.org/bots#how-do-i-create-a-bot)
   - `TELEGRAM_BOT_SECRET` set to a string of up to 256 characters, containing only `A-Z`, `a-z`, `0-9`, `-`, and `_`.
     This secret verifies that requests to AWS originate from your Telegram webhook.  

### Deployment Steps

#### Linux/macOS

1. Build .NET application.
   ```shell
   dotnet publish ./TunnelGPT --configuration Release --output ./publish --runtime linux-x64 --self-contained false
   ```
2. Create a .zip distribution.
   ```shell
   zip -r ./publish/TunnelGPT.zip ./publish/*
   ```
3. Initialize terraform.
   ```shell
   terraform -chdir=terraform init
   ```
4. Create resources in AWS.
   ```shell
   terraform -chdir=terraform apply \
       -var "openai_api_key=$OPENAI_API_KEY" \
       -var "telegram_bot_token=$TELEGRAM_BOT_TOKEN" \
       -var "telegram_bot_secret=$TELEGRAM_BOT_SECRET"
   ```
5. Set Telegram bot webhook.
   ```shell
   INVOKE_URL=$(terraform -chdir=terraform output -raw lambda_invoke_url)
   curl "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/setWebHook?url=$INVOKE_URL&secret_token=$TELEGRAM_BOT_SECRET"
   ```
6. Verify AWS setup.
   ```shell 
   data=$(cat <<EOF
   {
       "update_id": 101,
       "message": {
           "message_id": 201,
           "from": {
               "id": 301,
               "is_bot": false,
               "first_name": "John",
               "last_name": "Doe",
               "username": "johndoe",
               "language_code": "en"
           },
           "chat": {
               "id": 301,
               "first_name": "John",
               "last_name": "Doe",
               "username": "johndoe",
               "type": "private"
           },
           "date": 1735689600,
           "text": "Hello from John Doe!"
       }
   }
   EOF
   )
   INVOKE_URL=$(terraform -chdir=terraform output -raw lambda_invoke_url)
   curl $INVOKE_URL \
   -X POST \
   -H "Content-type: application/json" \
   -H "X-Telegram-Bot-Api-Secret-Token: $TELEGRAM_BOT_SECRET" \
   -d $data
   ```
   It should return `{"Status":"Error","Message":"Failed to process update: Bad Request: chat not found"}`.

#### Windows

1. Build .NET application.
   ```powershell
   dotnet publish ./TunnelGPT --configuration Release --output ./publish --runtime linux-x64 --self-contained false
   ```
2. Create a .zip distribution.
   ```powershell
   Compress-Archive -Path ./publish/* -DestinationPath ./publish/TunnelGPT.zip -Force
   ```
3. Initialize terraform.
   ```powershell
   terraform -chdir=terraform init
   ```
4. Create resources in AWS
   ```powershell
   terraform -chdir=terraform apply `
       -var "openai_api_key=$env:OPENAI_API_KEY" `
       -var "telegram_bot_token=$env:TELEGRAM_BOT_TOKEN" `
       -var "telegram_bot_secret=$env:TELEGRAM_BOT_SECRET"
   ```
5. Set Telegram bot webhook.
   ```powershell
   $invoke_url = terraform -chdir=terraform output -raw lambda_invoke_url
   curl "https://api.telegram.org/bot$env:TELEGRAM_BOT_TOKEN/setWebHook?url=$invoke_url&secret_token=$env:TELEGRAM_BOT_SECRET"
   ```
6. Verify AWS setup. 
   ```powershell
   $data = @"
   {
       "update_id": 101,
       "message": {
           "message_id": 201,
           "from": {
               "id": 301,
               "is_bot": false,
               "first_name": "John",
               "last_name": "Doe",
               "username": "johndoe",
               "language_code": "en"
           },
           "chat": {
               "id": 301,
               "first_name": "John",
               "last_name": "Doe",
               "username": "johndoe",
               "type": "private"
           },
           "date": 1735689600,
           "text": "Hello from John Doe!"
       }
   }
   "@
   $invoke_url = terraform -chdir=terraform output -raw lambda_invoke_url
   curl $invoke_url `
       -X POST `
       -H "Content-type: application/json" `
       -H "X-Telegram-Bot-Api-Secret-Token: $env:TELEGRAM_BOT_SECRET" `
       -d $data
   ```
   It should return `{"Status":"Error","Message":"Failed to process update: Bad Request: chat not found"}`.

## Remove

### Remover Requirements

See [Deployer Requirements](#deployer-requirements).

### Removal Steps

#### Linux/macOS

1. Remove AWS resources.
   ```shell
   terraform -chdir=terraform destroy \
       -var "openai_api_key=$OPENAI_API_KEY" \
       -var "telegram_bot_token=$TELEGRAM_BOT_TOKEN" \
       -var "telegram_bot_secret=$TELEGRAM_BOT_SECRET"
   ```
2. Unset Telegram webhook.
   ```shell
   curl "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/setWebHook"
   ```

#### Windows

1. Remove AWS resources.
   ```powershell
    terraform -chdir=terraform destroy `
       -var "openai_api_key=$env:OPENAI_API_KEY" `
       -var "telegram_bot_token=$env:TELEGRAM_BOT_TOKEN" `
       -var "telegram_bot_secret=$env:TELEGRAM_BOT_SECRET"
   ```
2. Unset Telegram webhook.
   ```powershell
   curl "https://api.telegram.org/bot$env:TELEGRAM_BOT_TOKEN/setWebHook"
   ```
