terraform {
  required_version = ">= 1.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  # Configure remote state
  # backend "azurerm" {
  #   resource_group_name  = "tfstate-rg"
  #   storage_account_name = "tfstatedev"
  #   container_name       = "tfstate"
  #   key                  = "lunchvote-dev.tfstate"
  # }
}

provider "azurerm" {
  features {}
}

# Reference shared modules here
# module "resource_group" {
#   source = "../../modules/resource-group"
#   environment = var.environment
#   location = var.location
# }
