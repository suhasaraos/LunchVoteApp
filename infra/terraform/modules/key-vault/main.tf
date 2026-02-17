# Key Vault Module
# Creates Azure Key Vault with RBAC authorization

resource "random_string" "suffix" {
  length  = 3
  special = false
  upper   = false
}

resource "azurerm_key_vault" "main" {
  name                          = "${var.key_vault_name}-${random_string.suffix.result}"
  location                      = var.location
  public_network_access_enabled = true
  purge_protection_enabled      = false
  resource_group_name           = var.resource_group_name
  sku_name                      = "standard"
  soft_delete_retention_days    = 7
  tenant_id                     = var.tenant_id
}
