output "lambda_invoke_url" {
  value = aws_api_gateway_stage.prod.invoke_url
  description = "The URL to invoke the API Gateway. Used to set up a Telegram webhook."
}

output "stage_arn" {
  value = aws_api_gateway_stage.prod.arn
  description = "The ARN of the API Gateway stage. Used set up WAF."
}
