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

module "api_gateway" {
  count  = var.lambda_is_deployed ? 1 : 0
  
  source = "./modules/api_gateway"
  lambda_function_name = var.application_name
}

module "waf" {
  count  = var.lambda_is_deployed ? 1 : 0
  depends_on = [module.api_gateway[0]]

  source = "./modules/waf"
  application_name = var.application_name
  telegram_bot_secret = var.telegram_bot_secret
  api_gateway_stage_arn = module.api_gateway[0].stage_arn
}
