variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "app_service_plan_name" { type = string }
variable "app_service_name" { type = string }
variable "sql_server_fqdn" { type = string }
variable "sql_database_name" { type = string }
variable "key_vault_uri" { type = string }
variable "environment" { type = string }

resource "azurerm_service_plan" "plan" {
  name                = var.app_service_plan_name
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "app" {
  name                = var.app_service_name
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.plan.id
  https_only          = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    always_on        = true
    ftps_state       = "Disabled"
    minimum_tls_version = "1.2"
    http2_enabled    = true
    
    cors {
      allowed_origins = [
        "http://localhost:5173",
        "http://localhost:3000",
        "https://*.azurestaticapps.net"
      ]
      support_credentials = false
    }
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = var.environment == "prod" ? "Production" : "Development"
    "KeyVaultUri"            = var.key_vault_uri
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Server=tcp:${var.sql_server_fqdn},1433;Database=${var.sql_database_name};Authentication=Active Directory Default;"
  }
}

output "app_service_name" { value = azurerm_linux_web_app.app.name }
output "default_hostname" { value = azurerm_linux_web_app.app.default_hostname }
output "principal_id" { value = azurerm_linux_web_app.app.identity[0].principal_id }
