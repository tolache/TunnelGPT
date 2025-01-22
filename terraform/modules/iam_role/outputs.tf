output "arn" {
  value       = aws_iam_role.tunnelgpt.arn
  description = "The ARN of the TunnelGPT function IAM role. Used by Lambda."
}
