resource "aws_iam_role" "tunnel_gpt" {
  name               = "TunnelGPT"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action    = "sts:AssumeRole"
        Effect    = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      },
    ]
  })
  description = "Role for the TunnelGPT Lambda function. Created using Terraform."
  tags = {
    Application = "TunnelGPT"
    Terraform   = "true"
  }
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.tunnel_gpt.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

output "tunnel_gpt_role_arn" {
  value       = aws_iam_role.tunnel_gpt.arn
  description = "The ARN of the TunnelGPT IAM role."
}
