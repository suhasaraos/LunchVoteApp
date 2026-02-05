output "app_service_name" {
  value = module.app_service.app_service_name
}

output "app_service_default_hostname" {
  value = module.app_service.default_hostname
}

output "app_service_principal_id" {
  value = module.app_service.principal_id
}

output "sql_server_fqdn" {
  value = module.sql_database.sql_server_fqdn
}

output "sql_database_name" {
  value = module.sql_database.database_name
}

output "key_vault_uri" {
  value = module.key_vault.key_vault_uri
}

output "static_web_app_default_hostname" {
  value = length(module.static_web_app) > 0 ? module.static_web_app[0].default_hostname : ""
}
