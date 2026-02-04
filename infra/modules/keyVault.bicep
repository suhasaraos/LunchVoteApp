// Key Vault Module
// Creates Azure Key Vault with RBAC authorization

@description('Azure region for the resources')
param location string

@description('Name of the Key Vault')
param keyVaultName string

//@description('App Service principal ID for role assignment')
//param appServicePrincipalId string

// Key Vault with RBAC authorization
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    // enablePurgeProtection: false (Cannot be disabled once enabled) // Set to true for production
    publicNetworkAccess: 'Enabled'
  }
}

// Role assignment: Key Vault Secrets User for App Service
// resource keyVaultSecretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(keyVault.id, appServicePrincipalId, 'Key Vault Secrets User')
//   scope: keyVault
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
//     principalId: appServicePrincipalId
//     principalType: 'ServicePrincipal'
//   }
// }

// Outputs
output keyVaultUri string = keyVault.properties.vaultUri
output keyVaultName string = keyVault.name
