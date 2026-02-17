
// Main orchestration template for Lunch Vote App infrastructure
// Deploys Backend API App Service, Frontend SPA App Service, SQL Database, and Key Vault

targetScope = 'resourceGroup'

@description('Environment suffix (dev, stg, prod)')
@allowed(['dev', 'stg', 'prod'])
param environment string = 'dev'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('Microsoft Entra ID object ID for SQL admin')
param sqlAdminObjectId string

@description('Microsoft Entra ID login name for SQL admin')
param sqlAdminLogin string

@description('Deploy Static Web App for SPA hosting')
param deployStaticWebApp bool = false

// Variables
var resourceSuffix = '-${environment}'
var backendAppServicePlanName = 'plan-lunchvote${resourceSuffix}'
var backendAppServiceName = 'app-lunchvote-api${resourceSuffix}'
var frontendAppServicePlanName = 'plan-lunchvote-spa${resourceSuffix}'
var sqlServerName = 'sql-lunchvote${resourceSuffix}'
var sqlDatabaseName = 'sqldb-lunchvote'
var keyVaultName = 'kv-lunchvote${resourceSuffix}'
var staticWebAppName = 'stapp-lunchvote${resourceSuffix}'

// Backend App Service Module
module appService 'modules/app-service.bicep' = {
  name: 'appServiceDeployment'
  params: {
    location: location
    appServicePlanName: backendAppServicePlanName
    appServiceName: backendAppServiceName
    sqlServerFqdn: sqlDatabase.outputs.sqlServerFqdn
    sqlDatabaseName: sqlDatabaseName
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

// SQL Database Module
module sqlDatabase 'modules/sql-database.bicep' = {
  name: 'sqlDatabaseDeployment'
  params: {
    location: location
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
    sqlAdminObjectId: sqlAdminObjectId
    sqlAdminLogin: sqlAdminLogin
  }
}

// Key Vault Module
module keyVault 'modules/key-vault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    location: location
    keyVaultName: keyVaultName
  }
}

// Frontend App Service Module
module frontendAppService 'modules/frontend-app-service.bicep' = {
  name: 'frontendAppServiceDeployment'
  params: {
    location: location
    appServicePlanName: frontendAppServicePlanName
    apiBaseUrl: 'https://${appService.outputs.defaultHostname}'
    environment: environment
  }
}

// Key Vault Access Assignment for Backend
module keyVaultAccess 'modules/key-vault-access.bicep' = {
  name: 'keyVaultAccessDeployment'
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    principalId: appService.outputs.principalId
  }
}

// Static Web App Module (optional)
module staticWebApp 'modules/static-web-app.bicep' = if (deployStaticWebApp) {
  name: 'staticWebAppDeployment'
  params: {
    location: location
    staticWebAppName: staticWebAppName
  }
}

// Outputs
output appServiceName string = appService.outputs.appServiceName
output appServiceDefaultHostname string = appService.outputs.defaultHostname
output appServicePrincipalId string = appService.outputs.principalId
output apiUrl string = 'https://${appService.outputs.defaultHostname}'
output frontendAppServiceName string = frontendAppService.outputs.appServiceName
output frontendAppServiceDefaultHostname string = frontendAppService.outputs.defaultHostname
output frontendAppServicePrincipalId string = frontendAppService.outputs.principalId
output frontendUrl string = frontendAppService.outputs.url
output sqlServerFqdn string = sqlDatabase.outputs.sqlServerFqdn
output sqlDatabaseName string = sqlDatabase.outputs.databaseName
output keyVaultUri string = keyVault.outputs.keyVaultUri
output staticWebAppDefaultHostname string = deployStaticWebApp ? staticWebApp!.outputs.defaultHostname : ''
