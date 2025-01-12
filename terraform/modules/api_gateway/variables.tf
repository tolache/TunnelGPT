variable "lambda_function_name" {
  type = string
  description = "The name of the TunnelGPT Lambda function. Used for extracting the existing Lambda function properties."
}

variable "lamda_invoke_arn" {
  type = string
  description = "The ARN of the TunnelGPT Lambda function to invoke."
}
