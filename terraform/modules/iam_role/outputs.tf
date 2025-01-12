output "arn" {
  value       = aws_iam_role.tunnel_gpt.arn
  description = "The ARN of the TunnelGPT function IAM role. Used by Lambda."
}
