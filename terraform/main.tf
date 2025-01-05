terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

variable "region" {
  default = "eu-west-1"
}

variable "lambda_is_deployed" {
  description = "Flag to determine if Lambda is deployed"
  type        = bool
  default     = false
}

provider "aws" {
  region  = var.region
}

module "iam_role" {
  source = "./modules/iam_role"
}

module "api_gateway" {
  count  = var.lambda_is_deployed ? 1 : 0
  
  source = "./modules/api_gateway"
}

output "lambda_invoke_url" {
  value = module.api_gateway[0].api_gateway_invoke_url
}
