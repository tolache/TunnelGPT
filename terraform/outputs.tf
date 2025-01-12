output "lambda_invoke_url" {
  value = module.api_gateway.lambda_invoke_url
  description = "URL to invoke the Lambda function. Use it to set a Telegram bot webhook."
}
