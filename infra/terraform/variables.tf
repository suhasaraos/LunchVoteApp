variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "eastus"
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod"
  }
}

variable "sql_admin_object_id" {
  description = "Entra ID Object ID for SQL Server admin"
  type        = string
}

variable "sql_admin_login" {
  description = "Entra ID login email for SQL Server admin"
  type        = string
}

variable "deploy_static_web_app" {
  description = "Whether to deploy Static Web App"
  type        = bool
  default     = true
}

variable "static_web_app_location" {
  description = "Azure region for Static Web App"
  type        = string
  default     = "eastus2"

  validation {
    condition     = contains(["westus2", "centralus", "eastus2", "westeurope", "eastasia"], var.static_web_app_location)
    error_message = "Static Web App location must be one of: westus2, centralus, eastus2, westeurope, eastasia"
  }
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
