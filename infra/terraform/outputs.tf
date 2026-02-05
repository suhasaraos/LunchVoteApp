output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.rg.name
}

output "sql_server_fqdn" {
  description = "Fully qualified domain name of the SQL Server"
  value       = module.sql_database.sql_server_fqdn
}

output "sql_database_name" {
  description = "Name of the SQL Database"
  value       = local.sql_database_name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = module.key_vault.key_vault_uri
}

output "app_service_name" {
  description = "Name of the App Service"
  value       = module.app_service.app_service_name
}

output "app_service_url" {
  description = "URL of the App Service"
  value       = module.app_service.app_service_url
}

output "static_web_app_url" {
  description = "URL of the Static Web App"
  value       = var.deploy_static_web_app ? module.static_web_app[0].default_hostname : null
}
