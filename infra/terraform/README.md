# Terraform Infrastructure

## Structure

```
terraform/
├── main.tf              # Main orchestration
├── variables.tf         # Input variables
├── outputs.tf           # Output values
├── terraform.tfvars     # Dev environment values
└── modules/             # Reusable modules
    ├── app-service/
    ├── sql-database/
    ├── key-vault/
    ├── key-vault-access/
    └── static-web-app/
```

## Prerequisites

1. [Terraform](https://www.terraform.io/downloads) >= 1.0
2. [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
3. Azure subscription

## Quick Start

```bash
# 1. Login to Azure
az login

# 2. Navigate to terraform directory
cd infra/terraform

# 3. Initialize Terraform
terraform init

# 4. Review the plan
terraform plan

# 5. Apply the configuration
terraform apply

# 6. View outputs
terraform output
```

## Configuration

Edit `terraform.tfvars` to customize your deployment:

```hcl
environment             = "dev"
location                = "australiaeast"
sql_admin_object_id     = "your-object-id"
sql_admin_login         = "your-email@domain.com"
deploy_static_web_app   = false
```

## Commands

```bash
# Initialize
terraform init

# Format code
terraform fmt -recursive

# Validate configuration
terraform validate

# Plan (preview changes)
terraform plan

# Apply changes
terraform apply

# Destroy resources
terraform destroy

# Show current state
terraform show

# List outputs
terraform output
```

## Module Dependencies

The modules are deployed in the following order:
1. SQL Database
2. Key Vault
3. App Service (depends on SQL and Key Vault)
4. Key Vault Access (depends on Key Vault and App Service)
5. Static Web App (optional)

## Notes

- Resource Group is created automatically as `rg-lunchvote-{environment}`
- All resources use passwordless authentication with Microsoft Entra ID
- Key Vault uses RBAC authorization (not access policies)
- Static Web App is optional and controlled by `deploy_static_web_app` variable
