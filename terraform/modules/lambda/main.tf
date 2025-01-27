resource "aws_lambda_function" "tunnelgpt" {
  architectures                  = ["x86_64"]
  description                    = "Handles TunnelGPT Telegram bot requests by invoking the OpenAI API"
  function_name                  = var.function_name
  handler                        = "TunnelGPT::TunnelGPT.Function::FunctionHandler"
  memory_size                    = 512
  package_type                   = "Zip"
  reserved_concurrent_executions = -1
  role                           = var.iam_role_arn
  runtime                        = "dotnet8"
  timeout                        = 30
  tags = {
    Application = var.function_name
    Terraform   = "true"
  }

  filename = "../publish/TunnelGPT.zip"
  source_code_hash = filebase64sha256("../publish/TunnelGPT.zip")

  environment {
    variables = {
      "OPENAI_API_KEY"     = var.openai_api_key
      "TELEGRAM_BOT_TOKEN" = var.telegram_bot_token
    }
  }

  logging_config {
    log_format            = "Text"
    log_group             = "/aws/lambda/${var.function_name}"
  }
}

resource "aws_cloudwatch_log_group" "lambda_log_group" {
  name              = "/aws/lambda/${var.function_name}"
  retention_in_days = 90
  tags = {
    Application = var.function_name
    Terraform   = "true"
  }
}
