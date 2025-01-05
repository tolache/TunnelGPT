output "lambda_role_name" {
  value       = aws_iam_role.tunnel_gpt.name
  description = "The name of the TunnelGPT function IAM role. Used for deploying Lambda function."
}
