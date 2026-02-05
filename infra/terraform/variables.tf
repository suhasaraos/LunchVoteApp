variable "environment" {
  description = "Environment suffix (dev, stg, prod)"
  type        = string
  default     = "dev"
  validation {
    condition     = contains(["dev", "stg", "prod"], var.environment)
    error_message = "Environment must be dev, stg, or prod."
  }
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "australiaeast"
}

variable "sql_admin_object_id" {
  description = "Microsoft Entra ID object ID for SQL admin"
  type        = string
}

variable "sql_admin_login" {
  description = "Microsoft Entra ID login name for SQL admin"
  type        = string
}

variable "deploy_static_web_app" {
  description = "Deploy Static Web App for SPA hosting"
  type        = bool
  default     = false
}
