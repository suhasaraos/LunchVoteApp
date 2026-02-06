// Key Vault Module
// Creates Azure Key Vault with RBAC authorization

@description('Azure region for the resources')
param location string

@description('Name of the Key Vault')
param keyVaultName string

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
    publicNetworkAccess: 'Enabled'
  }
}

// Outputs
output keyVaultUri string = keyVault.properties.vaultUri
output keyVaultName string = keyVault.name
