output "lambda_role_name" {
  value = module.iam_role.lambda_role_name
  description = "The name of the IAM role used by the Lambda function."
}

output "lambda_invoke_url" {
  value = var.lambda_is_deployed ? module.api_gateway[0].lambda_invoke_url : null
  description = "URL to invoke the Lambda function. Should be used to set a Telegram bot webhook."
}
