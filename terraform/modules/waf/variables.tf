variable "application_name" {
  type        = string
  description = "The name of the TunnelGPT Lambda function. Will be used for the WAF name."
}

variable "telegram_bot_secret" {
  type        = string
  description = "Telegram bot webhook's secret token. Used to verify that requests to AWS originate from your Telegram webhook."
}

variable "api_gateway_stage_arn" {
  type        = string
  description = "The ARN of the API Gateway stage."
}
