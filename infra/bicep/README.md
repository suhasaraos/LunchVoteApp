# Bicep Infrastructure

## Structure

```
bicep/
├── main.bicep              # Main template
├── parameters.dev.json     # Dev parameters
└── modules/                # Reusable modules
    ├── sql-database.bicep
    ├── key-vault.bicep
    ├── app-service.bicep
    ├── key-vault-access.bicep
    └── static-web-app.bicep
```

## Prerequisites

1. [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
2. Bicep CLI: `az bicep install`
3. Azure subscription

## Quick Start

```bash
# 1. Login
az login

# 2. Get credentials
az ad signed-in-user show --query id -o tsv          # Object ID
az ad signed-in-user show --query userPrincipalName -o tsv  # Email

# 3. Edit parameters
code parameters.dev.json

# 4. Create resource group
az group create --name rg-lunchvote-dev --location eastus

# 5. Deploy
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters parameters.dev.json
```

## Commands

```bash
# Build (syntax check)
az bicep build --file main.bicep

# What-if analysis
az deployment group what-if \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters parameters.dev.json

# Validate
az deployment group validate \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters parameters.dev.json

# Deploy
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters parameters.dev.json

# Delete
az group delete --name rg-lunchvote-dev --yes
```
