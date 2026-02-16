# Frontend App Service Module

This Terraform module creates an Azure App Service for hosting the React/Vite SPA frontend application.

## Resources Created

- **App Service Plan** (Linux, B1 SKU) - Dedicated hosting plan for the frontend
- **Linux Web App** - App Service configured for Node.js 20 LTS runtime
- **Random String** - Generates unique suffix for app service name to avoid conflicts

## Features

- **Node.js 20 LTS Runtime** - Configured for modern React/Vite applications
- **Managed Identity** - System-assigned identity for secure access to Azure resources
- **HTTPS Only** - Enforces secure connections
- **Build During Deployment** - SCM build enabled for npm/vite builds
- **Environment Variables** - Configured with API base URL for frontend-backend communication

## Inputs

| Name | Description | Type | Required |
|------|-------------|------|----------|
| location | Azure region for the resources | string | Yes |
| resource_group_name | Name of the resource group | string | Yes |
| app_service_plan_name | Name of the App Service Plan | string | Yes |
| api_base_url | Base URL of the API backend | string | Yes |
| environment | Environment suffix (dev/stg/prod) | string | Yes |

## Outputs

| Name | Description |
|------|-------------|
| app_service_name | Name of the Frontend App Service |
| default_hostname | Default hostname of the Frontend App Service |
| principal_id | Principal ID of the managed identity |
| url | Full HTTPS URL of the Frontend App Service |

## Usage

```hcl
module "frontend_app_service" {
  source = "./modules/frontend-app-service"
  
  location               = "australiaeast"
  resource_group_name    = "rg-lunchvote-dev"
  app_service_plan_name  = "plan-lunchvote-spa-dev"
  api_base_url           = "https://app-lunchvote-api-dev.azurewebsites.net"
  environment            = "dev"
}
```

## Deployment Notes

1. **Build Configuration**: The app service is configured with `SCM_DO_BUILD_DURING_DEPLOYMENT=true`, which means it will run `npm install` and `npm run build` during deployment.

2. **Environment Variables**: The `VITE_API_BASE_URL` environment variable is automatically set and available during build time for Vite to use.

3. **Deployment Methods**: 
   - Deploy via Azure CLI: `az webapp deployment source config-zip`
   - Deploy via GitHub Actions
   - Deploy via Azure DevOps pipelines

4. **CORS**: CORS should be configured on the API App Service to allow requests from this frontend URL.
