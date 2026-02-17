output "app_service_name" {
  description = "Name of the Frontend App Service"
  value       = azurerm_linux_web_app.frontend.name
}

output "default_hostname" {
  description = "Default hostname of the Frontend App Service"
  value       = azurerm_linux_web_app.frontend.default_hostname
}

output "principal_id" {
  description = "Principal ID of the Frontend App Service managed identity"
  value       = azurerm_linux_web_app.frontend.identity[0].principal_id
}

output "url" {
  description = "Full HTTPS URL of the Frontend App Service"
  value       = "https://${azurerm_linux_web_app.frontend.default_hostname}"
}
