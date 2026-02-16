# Key Vault Module
# Creates Azure Key Vault with RBAC authorization

resource "azurerm_key_vault" "main" {
  name                          = var.key_vault_name
  location                      = var.location
  public_network_access_enabled = true
  purge_protection_enabled      = false
  resource_group_name           = var.resource_group_name
  sku_name                      = "standard"
  soft_delete_retention_days    = 7
  tenant_id                     = var.tenant_id
}
