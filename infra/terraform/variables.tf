variable "resource_group_name" {
  type        = string
  description = "Name of the existing resource group"
}

variable "environment" {
  type        = string
  description = "Environment suffix (dev, stg, prod)"
  validation {
    condition     = contains(["dev", "stg", "prod"], var.environment)
    error_message = "Environment must be one of: dev, stg, prod."
  }
  default = "dev"
}

variable "location" {
  type        = string
  description = "Azure region for resources"
}

variable "sql_admin_object_id" {
  type        = string
  description = "Microsoft Entra ID object ID for SQL admin"
}

variable "sql_admin_login" {
  type        = string
  description = "Microsoft Entra ID login name for SQL admin"
}

variable "deploy_static_web_app" {
  type        = bool
  description = "Deploy Static Web App for SPA hosting"
  default     = false
}
