// Main orchestration template for Lunch Vote App infrastructure
// Deploys App Service, SQL Database, Key Vault, and Static Web App

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
var appServicePlanName = 'plan-lunchvote${resourceSuffix}'
var appServiceName = 'app-lunchvote-api${resourceSuffix}'
var sqlServerName = 'sql-lunchvote${resourceSuffix}'
var sqlDatabaseName = 'sqldb-lunchvote'
var keyVaultName = 'kv-lunchvote${resourceSuffix}'
var staticWebAppName = 'stapp-lunchvote${resourceSuffix}'

// App Service Module
module appService 'modules/appService.bicep' = {
  name: 'appServiceDeployment'
  params: {
    location: location
    appServicePlanName: appServicePlanName
    appServiceName: appServiceName
    sqlServerFqdn: sqlDatabase.outputs.sqlServerFqdn
    sqlDatabaseName: sqlDatabaseName
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

// SQL Database Module
module sqlDatabase 'modules/sqlDatabase.bicep' = {
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
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    location: location
    keyVaultName: keyVaultName
    appServicePrincipalId: appService.outputs.principalId
  }
}

// Static Web App Module (optional)
module staticWebApp 'modules/staticWebApp.bicep' = if (deployStaticWebApp) {
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
output sqlServerFqdn string = sqlDatabase.outputs.sqlServerFqdn
output sqlDatabaseName string = sqlDatabase.outputs.databaseName
output keyVaultUri string = keyVault.outputs.keyVaultUri
output staticWebAppDefaultHostname string = deployStaticWebApp ? staticWebApp.outputs.defaultHostname : ''
