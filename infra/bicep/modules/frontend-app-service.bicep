// Frontend App Service Module
// Creates App Service Plan and App Service for the React/Vite SPA

@description('Azure region for the resources')
param location string

@description('Name of the App Service Plan for frontend')
param appServicePlanName string

@description('Base URL of the API backend')
param apiBaseUrl string

@description('Environment suffix')
param environment string

// Frontend App Service Plan (Linux, B1 SKU)
resource frontendAppServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux
  }
}

// Frontend App Service
resource frontendAppService 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-lunchvote-spa-${environment}'
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: frontendAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|20-lts'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      // Enable SPA routing - all routes should serve index.html
      appCommandLine: 'pm2 serve /home/site/wwwroot/dist --no-daemon --spa'
      appSettings: [
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~20'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'true'
        }
        {
          name: 'VITE_API_BASE_URL'
          value: apiBaseUrl
        }
        {
          name: 'PRE_BUILD_COMMAND'
          value: 'npm install -g pm2 serve'
        }
      ]
    }
  }
}

// Outputs
output appServiceName string = frontendAppService.name
output defaultHostname string = frontendAppService.properties.defaultHostName
output principalId string = frontendAppService.identity.principalId
output url string = 'https://${frontendAppService.properties.defaultHostName}'
