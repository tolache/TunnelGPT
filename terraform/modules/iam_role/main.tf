resource "aws_iam_role" "tunnelgpt" {
  name               = var.application_name
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
  description = "Role for the ${var.application_name} Lambda function. Created using Terraform."
  tags = {
    Application = var.application_name
    Terraform   = "true"
  }
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.tunnelgpt.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy_attachment" "dynamodb_access" {
  role       = aws_iam_role.tunnelgpt.name
  policy_arn = aws_iam_policy.dynamodb_access.arn
}

resource "aws_iam_policy" "dynamodb_access" {
  name        = "DynamoDBAccessPolicy"
  description = "Policy for accessing ${var.application_name} DynamoDB table"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect   = "Allow",
        Action   = [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:DescribeTable"
        ],
        Resource = var.dynamodb_table_arn
      }
    ]
  })

  tags = {
    Application = var.application_name
    Terraform   = "true"
  }
}
