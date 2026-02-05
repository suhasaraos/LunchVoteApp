variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "sql_server_name" { type = string }
variable "sql_database_name" { type = string }
variable "sql_admin_object_id" { type = string }
variable "sql_admin_login" { type = string }
variable "tenant_id" { type = string }

resource "azurerm_mssql_server" "server" {
  name                         = var.sql_server_name
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  /* administrator_login removed to enable AD-only auth */
  
  azuread_administrator {
    login_username = var.sql_admin_login
    object_id      = var.sql_admin_object_id
    tenant_id      = var.tenant_id
    azuread_authentication_only = true
  }
  minimum_tls_version          = "1.2"
  public_network_access_enabled = true
}

resource "azurerm_mssql_database" "db" {
  name           = var.sql_database_name
  server_id      = azurerm_mssql_server.server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  sku_name       = "Basic"
  max_size_gb    = 2
  zone_redundant = false
}

resource "azurerm_mssql_firewall_rule" "allow_azure_ips" {
  name             = "AllowAllWindowsAzureIps"
  server_id        = azurerm_mssql_server.server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

output "sql_server_fqdn" { value = azurerm_mssql_server.server.fully_qualified_domain_name }
output "database_name" { value = azurerm_mssql_database.db.name }
