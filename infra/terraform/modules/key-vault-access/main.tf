# Key Vault Access Module
# Assigns 'Key Vault Secrets User' role to a principal

resource "azurerm_role_assignment" "key_vault_secrets_user" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.principal_id
}
