using 'main.bicep'

param environment = 'dev'
param location = 'australiaeast'

// Replace with your Microsoft Entra ID object ID and login
param sqlAdminObjectId = '<your-entra-id-object-id>'
param sqlAdminLogin = '<your-entra-id-login-email>'

param deployStaticWebApp = false
