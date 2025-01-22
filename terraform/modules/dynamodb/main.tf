resource "aws_dynamodb_table" "tunnelgpt" {
  name           = "tunnelgpt"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "user_id"

  attribute {
    name = "user_id"
    type = "N"
  }
  
  tags = {
    Application = var.application_name
    Terraform   = "true"
  }
}
