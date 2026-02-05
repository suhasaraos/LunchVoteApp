// Static Web App Module
// Creates Azure Static Web App for SPA hosting (optional)

@description('Azure region for the resources')
param location string

@description('Name of the Static Web App')
param staticWebAppName string

// Static Web App (Free tier)
resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    buildProperties: {
      appLocation: '/src/lunch-vote-spa'
      apiLocation: ''
      outputLocation: 'dist'
      appBuildCommand: 'npm run build'
    }
  }
}

// Outputs
output defaultHostname string = staticWebApp.properties.defaultHostname
output staticWebAppName string = staticWebApp.name
