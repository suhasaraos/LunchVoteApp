# Static Web App Module
# Creates Azure Static Web App for SPA hosting (optional)

resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_static_web_app" "main" {
  name                = "${var.static_web_app_name}-${random_string.suffix.result}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku_tier            = "Free"
  sku_size            = "Free"
}
