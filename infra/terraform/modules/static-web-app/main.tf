# Static Web App Module
# Creates Azure Static Web App for SPA hosting (optional)

resource "azurerm_static_web_app" "main" {
  name                = var.static_web_app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku_tier            = "Free"
  sku_size            = "Free"
}
