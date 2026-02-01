// App Service Module
// Creates App Service Plan and App Service for the .NET 10 API

@description('Azure region for the resources')
param location string

@description('Name of the App Service Plan')
param appServicePlanName string

@description('Name of the App Service')
param appServiceName string

@description('SQL Server fully qualified domain name')
param sqlServerFqdn string

@description('SQL Database name')
param sqlDatabaseName string

@description('Key Vault URI for secret references')
param keyVaultUri string

@description('Environment suffix')
param environment string

// App Service Plan (Linux, B1 SKU)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
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

// App Service
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'KeyVaultUri'
          value: keyVaultUri
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServerFqdn},1433;Database=${sqlDatabaseName};Authentication=Active Directory Default;'
          type: 'SQLAzure'
        }
      ]
    }
  }
}

// Configure CORS
resource appServiceCors 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: appService
  name: 'web'
  properties: {
    cors: {
      allowedOrigins: [
        'http://localhost:5173'
        'http://localhost:3000'
        'https://*.azurestaticapps.net'
      ]
      supportCredentials: false
    }
  }
}

// Outputs
output appServiceName string = appService.name
output defaultHostname string = appService.properties.defaultHostName
output principalId string = appService.identity.principalId
