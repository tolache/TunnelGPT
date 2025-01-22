variable "application_name" {
  type = string
  description = "Application name. Used as the IAM role name, in descriptions, and tags."
}

variable "dynamodb_table_arn" {
  type = string
  description = "DynamoDB table ARN"
}
