output "table_arn" {
  value = aws_dynamodb_table.tunnelgpt.arn
  description = "DynamoDB table ARN. Used by IAM role policy."
}
