terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
}

data "azurerm_client_config" "current" {}

locals {
  resource_suffix      = "-${var.environment}"
  app_service_plan_name = "plan-lunchvote${local.resource_suffix}"
  app_service_name      = "app-lunchvote-api${local.resource_suffix}"
  sql_server_name       = "sql-lunchvote${local.resource_suffix}"
  sql_database_name     = "sqldb-lunchvote"
  key_vault_name        = "kv-lunchvote${local.resource_suffix}"
  static_web_app_name   = "stapp-lunchvote${local.resource_suffix}"
}

module "sql_database" {
  source              = "./modules/sql_database"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  sql_server_name     = local.sql_server_name
  sql_database_name   = local.sql_database_name
  sql_admin_object_id = var.sql_admin_object_id
  sql_admin_login     = var.sql_admin_login
  tenant_id           = data.azurerm_client_config.current.tenant_id
}

module "key_vault" {
  source              = "./modules/key_vault"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  key_vault_name      = local.key_vault_name
  tenant_id           = data.azurerm_client_config.current.tenant_id
}

module "app_service" {
  source                = "./modules/app_service"
  resource_group_name   = azurerm_resource_group.rg.name
  location              = var.location
  app_service_plan_name = local.app_service_plan_name
  app_service_name      = local.app_service_name
  sql_server_fqdn       = module.sql_database.sql_server_fqdn
  sql_database_name     = local.sql_database_name
  key_vault_uri         = module.key_vault.key_vault_uri
  environment           = var.environment
}

module "key_vault_access" {
  source         = "./modules/key_vault_access"
  key_vault_id   = module.key_vault.key_vault_id
  principal_id   = module.app_service.principal_id
}

module "static_web_app" {
  count                = var.deploy_static_web_app ? 1 : 0
  source               = "./modules/static_web_app"
  resource_group_name  = azurerm_resource_group.rg.name
  location             = var.location
  static_web_app_name  = local.static_web_app_name
}
