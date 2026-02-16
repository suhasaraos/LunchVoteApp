# Main orchestration for Lunch Vote App infrastructure
# Deploys App Service, SQL Database, Key Vault, and Static Web App

terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
      recover_soft_deleted_key_vaults = true
    }
  }
}

# Data source for current Azure configuration
data "azurerm_client_config" "current" {}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-lunchvote-${var.environment}"
  location = var.location
}

# SQL Database Module
module "sql_database" {
  source = "./modules/sql-database"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sql_server_name     = "sql-lunchvote-${var.environment}"
  sql_database_name   = "sqldb-lunchvote"
  sql_admin_object_id = var.sql_admin_object_id
  sql_admin_login     = var.sql_admin_login
  tenant_id           = data.azurerm_client_config.current.tenant_id
}

# Key Vault Module
module "key_vault" {
  source = "./modules/key-vault"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  key_vault_name      = "kv-lunchvote-${var.environment}"
  tenant_id           = data.azurerm_client_config.current.tenant_id
}

# API App Service Module
module "app_service" {
  source = "./modules/app-service"
  location               = azurerm_resource_group.main.location
  resource_group_name    = azurerm_resource_group.main.name
  app_service_plan_name  = "plan-lunchvote-${var.environment}"
  app_service_name       = "app-lunchvote-api-${var.environment}"
  sql_server_fqdn        = module.sql_database.sql_server_fqdn
  sql_database_name      = module.sql_database.database_name
  key_vault_uri          = module.key_vault.key_vault_uri
  environment            = var.environment
}

# Frontend App Service Module
module "frontend_app_service" {
  source = "./modules/frontend-app-service"
  location               = azurerm_resource_group.main.location
  resource_group_name    = azurerm_resource_group.main.name
  app_service_plan_name  = "plan-lunchvote-spa-${var.environment}"
  api_base_url           = "https://${module.app_service.default_hostname}"
  environment            = var.environment
}

# Key Vault Access Module
module "key_vault_access" {
  source = "./modules/key-vault-access"
  key_vault_id = module.key_vault.key_vault_id
  principal_id = module.app_service.principal_id
}

# Static Web App Module (optional - only if not using App Service for frontend)
module "static_web_app" {
  count  = var.deploy_static_web_app ? 1 : 0
  source = "./modules/static-web-app"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  static_web_app_name = "stapp-lunchvote-${var.environment}"
}
