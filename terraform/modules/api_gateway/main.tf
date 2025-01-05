data "aws_lambda_function" "lambda" {
  function_name = "TunnelGPT"
}

output "api_gateway_invoke_url" {
  value = aws_api_gateway_stage.prod.invoke_url
  description = "The URL to invoke the API Gateway"
}

resource "aws_api_gateway_rest_api" "api" {
  name        = "TunnelGPT"
  description = "API Gateway for TunnelGPT Lambda function. Created using Terraform."
  
  tags = {
    Application = "TunnelGPT"
    Terraform   = "true"
  }
}

resource "aws_api_gateway_method" "any_method" {
  rest_api_id   = aws_api_gateway_rest_api.api.id
  resource_id   = aws_api_gateway_rest_api.api.root_resource_id
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda" {
  rest_api_id             = aws_api_gateway_rest_api.api.id
  resource_id             = aws_api_gateway_rest_api.api.root_resource_id
  http_method             = aws_api_gateway_method.any_method.http_method
  integration_http_method = "POST"
  type                    = "AWS"
  uri                     = data.aws_lambda_function.lambda.invoke_arn

  passthrough_behavior = "WHEN_NO_MATCH"
  content_handling     = "CONVERT_TO_TEXT"
}

resource "aws_api_gateway_method_response" "default_response" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_rest_api.api.root_resource_id
  http_method = aws_api_gateway_method.any_method.http_method
  status_code = "200"

  response_models = {
    "application/json" = "Empty"
  }
}

resource "aws_api_gateway_integration_response" "default_response" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_rest_api.api.root_resource_id
  http_method = aws_api_gateway_method.any_method.http_method
  status_code = aws_api_gateway_method_response.default_response.status_code
  
  response_templates = {
    "application/json" = ""
  }
}

resource "aws_api_gateway_deployment" "api_deployment" {
  depends_on = [
    aws_api_gateway_method.any_method,
    aws_api_gateway_integration.lambda
  ]

  rest_api_id = aws_api_gateway_rest_api.api.id
  description = "API Gateway Deployment for TunnelGPT Lambda function. Created using Terraform."
}

resource "aws_api_gateway_stage" "prod" {
  stage_name    = "prod"
  deployment_id = aws_api_gateway_deployment.api_deployment.id
  rest_api_id   = aws_api_gateway_rest_api.api.id
  description = "API Gateway Stage for TunnelGPT Lambda function. Created using Terraform."
  
  tags = {
    Application = "TunnelGPT"
    Terraform   = "true"
  }
}

resource "aws_lambda_permission" "api_gateway" {
  depends_on = [aws_api_gateway_deployment.api_deployment]
  
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = data.aws_lambda_function.lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn = "${aws_api_gateway_rest_api.api.execution_arn}/*/*"
}
