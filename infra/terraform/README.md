# Lunch Vote App - Nonprod Configuration

Terraform configuration for deploying the Lunch Vote App infrastructure to Azure nonprod environment.

## What this deploys

This configuration deploys a complete Lunch Vote application infrastructure including:

- Azure Resource Group
- Azure SQL Database with Microsoft Entra ID authentication
- Azure App Service (Linux) for .NET API
- Azure Key Vault for secrets management
- Azure Static Web App (optional) for frontend
- Managed Identity and RBAC permissions

## Prerequisites

1. [Terraform 1.11.4](https://www.terraform.io/downloads)
2. [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
3. Azure subscription with appropriate permissions
4. Azure Storage Account for Terraform state (see Backend Configuration below)

## Backend Configuration

Before deploying, ensure you have:

- Resource group: `rg-terraform-state`
- Storage account: `sttfstatelunchvote` (update in versions.tf if different)
- Container: `tfstate`

Create backend resources if they don't exist:

```bash
az group create --name rg-terraform-state --location australiaeast
az storage account create --name sttfstatelunchvote --resource-group rg-terraform-state --location australiaeast --sku Standard_LRS
az storage container create --name tfstate --account-name sttfstatelunchvote
```

## Deployment Instructions

### 1. Configure Variables

Edit `terraform.tfvars`:

```hcl
name                  = "lunchvote"
env                   = "nonprod"
location              = "australiaeast"
sql_admin_object_id   = "your-entra-id-object-id"
sql_admin_login       = "your-email@company.com"
deploy_static_web_app = false
```

Get your Object ID:

```bash
az ad signed-in-user show --query id -o tsv
```

### 2. Login to Azure

```bash
az login
az account set --subscription "your-subscription-id"
```

### 3. Deploy

```bash
cd infra/terraform
terraform init
terraform plan
terraform apply
```

### 4. Post-Deployment

After deployment, configure the SQL database user permissions as needed. Connection details are available in outputs:

```bash
terraform output
```

## Outputs

The configuration provides the following outputs:

- `app_service_name` - Name of the deployed App Service
- `app_service_default_hostname` - URL of the App Service
- `sql_server_fqdn` - SQL Server fully qualified domain name
- `sql_database_name` - Name of the SQL Database
- `key_vault_uri` - URI of the Key Vault
- `static_web_app_default_hostname` - URL of the Static Web App (if deployed)

## Notes

- All resources use passwordless authentication with Microsoft Entra ID
- Key Vault uses RBAC authorization (not access policies)
- Default tags are applied via provider configuration
- Backend state is encrypted and uses table locking
- Resource naming follows pattern: `{resource-type}-{name}-{env}`

## Contact

For issues or questions, contact: Engineering Team
