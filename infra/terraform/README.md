# Terraform Infrastructure

## Structure

```
terraform/
├── main.tf                   # Main configuration
├── variables.tf              # Variable definitions
├── outputs.tf                # Output definitions
├── terraform.tfvars.example  # Example variables
└── modules/                  # Reusable modules
    ├── sql_database/
    ├── key_vault/
    ├── app_service/
    ├── key_vault_access/
    └── static_web_app/
```

## Prerequisites

1. [Terraform](https://www.terraform.io/downloads) >= 1.0
2. [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
3. Azure subscription

## Setup

```bash
# 1. Login to Azure
az login

# 2. Copy example variables
cp terraform.tfvars.example terraform.tfvars

# 3. Get your credentials
az ad signed-in-user show --query id -o tsv          # Object ID
az ad signed-in-user show --query userPrincipalName -o tsv  # Email

# 4. Edit terraform.tfvars with your values
code terraform.tfvars

# 5. Initialize Terraform
terraform init

# 6. Review the plan
terraform plan

# 7. Apply
terraform apply
```

## Commands

```bash
# Format code
terraform fmt

# Validate configuration
terraform validate

# Plan changes
terraform plan -out=tfplan

# Apply changes
terraform apply tfplan

# Destroy resources
terraform destroy

# Show current state
terraform show

# List resources
terraform state list
```

## Modules

Each module is self-contained and reusable:

- **sql_database**: Azure SQL Server + Database with Entra ID auth
- **key_vault**: Azure Key Vault with RBAC
- **app_service**: App Service Plan + Web App with managed identity
- **key_vault_access**: RBAC assignment for Key Vault access
- **static_web_app**: Azure Static Web App (optional)
