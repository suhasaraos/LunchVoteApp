variable "location" {
  description = "Azure region for the resources"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "sql_server_name" {
  description = "Name of the SQL Server"
  type        = string
}

variable "sql_database_name" {
  description = "Name of the SQL Database"
  type        = string
}

variable "sql_admin_object_id" {
  description = "Microsoft Entra ID object ID for SQL admin"
  type        = string
}

variable "sql_admin_login" {
  description = "Microsoft Entra ID login name for SQL admin"
  type        = string
}

variable "tenant_id" {
  description = "Azure AD tenant ID"
  type        = string
}
