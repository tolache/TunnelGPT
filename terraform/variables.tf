variable "aws_region" {
  default = "eu-west-1"
}

variable "application_name" {
  default = "TunnelGPT"
  description = "Name of the application. Should be the same as that of the Lambda function deployed by `dotnet lambda deploy-function` as it will be used for retrieving the Lambda function properties. It will also be used as the name of resources like IAM role used by Lambda, and API Gateway."
}

variable "lambda_is_deployed" {
  description = "Flag to determine if Lambda is deployed. Needs to be set to true after running `dotnet lambda deploy-function`."
  type        = bool
  default     = false
}

variable "telegram_bot_secret" {
  type        = string
  description = "Telegram bot webhook's secret token. Used to verify that requests to AWS originate from your Telegram webhook."
}
