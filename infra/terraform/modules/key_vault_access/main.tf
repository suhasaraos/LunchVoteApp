variable "key_vault_id" { type = string }
variable "principal_id" { type = string }

resource "azurerm_role_assignment" "kv_user" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.principal_id
}
