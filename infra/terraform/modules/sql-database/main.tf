# SQL Database Module
# Creates Azure SQL Server and Database with Microsoft Entra-only authentication

resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_mssql_server" "main" {
  name                          = "${var.sql_server_name}-${random_string.suffix.result}"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  version                       = "12.0"
  minimum_tls_version           = "1.2"
  # Public access enabled for nonprod development convenience; use Private Endpoints in prod
  public_network_access_enabled = true

  azuread_administrator {
    azuread_authentication_only = true
    login_username              = var.sql_admin_login
    object_id                   = var.sql_admin_object_id
  }
}

resource "azurerm_mssql_database" "main" {
  name           = "${var.sql_database_name}-${random_string.suffix.result}"
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb    = 2
  read_scale     = false
  sku_name       = "Basic"
  zone_redundant = false
}

# Firewall rule to allow Azure services
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAllWindowsAzureIps"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
