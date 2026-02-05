variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "static_web_app_name" { type = string }

resource "azurerm_static_web_app" "swa" {
  name                = var.static_web_app_name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = "Free"
  sku_size            = "Free"
}

output "default_hostname" { value = azurerm_static_web_app.swa.default_host_name }
output "static_web_app_name" { value = azurerm_static_web_app.swa.name }
