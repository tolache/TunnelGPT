resource "aws_iam_role" "tunnel_gpt" {
  name               = var.lambda_function_name
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
  description = "Role for the ${var.lambda_function_name} Lambda function. Created using Terraform."
  tags = {
    Application = var.lambda_function_name
    Terraform   = "true"
  }
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.tunnel_gpt.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}
