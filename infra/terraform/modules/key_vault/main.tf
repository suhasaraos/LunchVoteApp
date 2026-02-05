variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "key_vault_name" { type = string }
variable "tenant_id" { type = string }

resource "azurerm_key_vault" "kv" {
  name                        = var.key_vault_name
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = false
  tenant_id                   = var.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false 

  sku_name = "standard"

  enable_rbac_authorization = true
  public_network_access_enabled = true
}

output "key_vault_uri" { value = azurerm_key_vault.kv.vault_uri }
output "key_vault_name" { value = azurerm_key_vault.kv.name }
output "key_vault_id" { value = azurerm_key_vault.kv.id }
