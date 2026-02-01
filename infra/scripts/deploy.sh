#!/bin/bash
# Deployment script for Lunch Vote App infrastructure

set -e

# Configuration
RESOURCE_GROUP="rg-lunchvote-dev"
LOCATION="australiaeast"
ENVIRONMENT="dev"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  Lunch Vote App - Infrastructure Deployment${NC}"
echo -e "${GREEN}========================================${NC}"

# Check if logged in to Azure
echo -e "\n${YELLOW}Checking Azure CLI login status...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${RED}Not logged in to Azure. Please run 'az login' first.${NC}"
    exit 1
fi

ACCOUNT_NAME=$(az account show --query name -o tsv)
echo -e "${GREEN}Logged in to: ${ACCOUNT_NAME}${NC}"

# Get current user's object ID for SQL admin
echo -e "\n${YELLOW}Getting current user information...${NC}"
USER_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
USER_EMAIL=$(az ad signed-in-user show --query userPrincipalName -o tsv)
echo -e "${GREEN}User: ${USER_EMAIL}${NC}"
echo -e "${GREEN}Object ID: ${USER_OBJECT_ID}${NC}"

# Create resource group
echo -e "\n${YELLOW}Creating resource group...${NC}"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none
echo -e "${GREEN}Resource group created: ${RESOURCE_GROUP}${NC}"

# Deploy Bicep template
echo -e "\n${YELLOW}Deploying infrastructure...${NC}"
DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --parameters environment="$ENVIRONMENT" \
    --parameters location="$LOCATION" \
    --parameters sqlAdminObjectId="$USER_OBJECT_ID" \
    --parameters sqlAdminLogin="$USER_EMAIL" \
    --query properties.outputs \
    --output json)

echo -e "${GREEN}Infrastructure deployed successfully!${NC}"

# Extract outputs
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceName.value')
APP_SERVICE_HOSTNAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceDefaultHostname.value')
APP_SERVICE_PRINCIPAL_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServicePrincipalId.value')
SQL_SERVER_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerFqdn.value')
SQL_DATABASE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlDatabaseName.value')
KEY_VAULT_URI=$(echo $DEPLOYMENT_OUTPUT | jq -r '.keyVaultUri.value')

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}  Deployment Outputs${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "App Service Name: ${APP_SERVICE_NAME}"
echo -e "App Service URL: https://${APP_SERVICE_HOSTNAME}"
echo -e "App Service Principal ID: ${APP_SERVICE_PRINCIPAL_ID}"
echo -e "SQL Server FQDN: ${SQL_SERVER_FQDN}"
echo -e "SQL Database Name: ${SQL_DATABASE_NAME}"
echo -e "Key Vault URI: ${KEY_VAULT_URI}"

echo -e "\n${YELLOW}========================================${NC}"
echo -e "${YELLOW}  Post-Deployment Steps${NC}"
echo -e "${YELLOW}========================================${NC}"
echo -e "1. Run the SQL user creation script to grant access to the App Service managed identity:"
echo -e "   ${GREEN}sqlcmd -S ${SQL_SERVER_FQDN} -d ${SQL_DATABASE_NAME} -G -i create-sql-user.sql${NC}"
echo -e ""
echo -e "2. Deploy the API:"
echo -e "   ${GREEN}cd ../src/LunchVoteApi && dotnet publish -c Release -o ./publish${NC}"
echo -e "   ${GREEN}az webapp deploy --resource-group ${RESOURCE_GROUP} --name ${APP_SERVICE_NAME} --src-path ./publish --type zip${NC}"
echo -e ""
echo -e "3. Run EF migrations:"
echo -e "   ${GREEN}dotnet ef database update --connection \"Server=tcp:${SQL_SERVER_FQDN},1433;Database=${SQL_DATABASE_NAME};Authentication=Active Directory Default;\"${NC}"

echo -e "\n${GREEN}Deployment complete!${NC}"
