terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {}

module "iam_role" {
  source = "./modules/iam_role"
  lambda_function_name = var.application_name
}

module "lambda" {
  depends_on = [module.iam_role]
  
  source = "./modules/lambda"
  function_name = var.application_name
  iam_role_arn = module.iam_role.arn
  openai_api_key = var.openai_api_key
  telegram_bot_token = var.telegram_bot_token
}

module "api_gateway" {
  depends_on = [module.lambda]
  
  source = "./modules/api_gateway"
  lambda_function_name = module.lambda.function_name
  lamda_invoke_arn = module.lambda.invoke_arn
}

module "waf" {
  depends_on = [module.api_gateway]

  source = "./modules/waf"
  application_name = var.application_name
  telegram_bot_secret = var.telegram_bot_secret
  api_gateway_stage_arn = module.api_gateway.stage_arn
}
