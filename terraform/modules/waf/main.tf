resource "aws_wafv2_web_acl" "api_waf" {
  name        = var.application_name
  scope       = "REGIONAL"
  description = "WAF for validating Telegram webhook requests. Created using Terraform."

  default_action {
    block {}
  }

  rule {
    name     = "ValidateTelegramBotSecret"
    priority = 0

    statement {
      byte_match_statement {
        field_to_match {
          single_header {
            name = "x-telegram-bot-api-secret-token"
          }
        }

        positional_constraint = "EXACTLY"
        search_string         = var.telegram_bot_secret

        text_transformation {
          priority = 0
          type     = "NONE"
        }
      }
    }

    action {
      allow {}
    }

    visibility_config {
      sampled_requests_enabled    = true
      cloudwatch_metrics_enabled  = true
      metric_name                 = "${var.application_name}_ValidateTelegramBotSecret"
    }
  }

  visibility_config {
    sampled_requests_enabled    = true
    cloudwatch_metrics_enabled  = true
    metric_name                 = "${var.application_name}_WebACL"
  }

  tags = {
    Application = var.application_name
    Terraform   = "true"
  }
}

resource "aws_wafv2_web_acl_association" "api_stage_association" {
  resource_arn = var.api_gateway_stage_arn
  web_acl_arn  = aws_wafv2_web_acl.api_waf.arn
}
