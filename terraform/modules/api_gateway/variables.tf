variable "lambda_function_name" {
  type = string
  description = "Lambda function name. Used for description and tags in API Gateway and related resources."
}

variable "lamda_invoke_arn" {
  type = string
  description = "The ARN of the TunnelGPT Lambda function to invoke."
}
