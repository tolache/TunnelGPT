output "function_name" {
  value = aws_lambda_function.tunnel_gpt.function_name
}

output "invoke_arn" {
  value = aws_lambda_function.tunnel_gpt.invoke_arn
  description = "The invoke ARN of the Lambda function. Used by API Gateway."
}
