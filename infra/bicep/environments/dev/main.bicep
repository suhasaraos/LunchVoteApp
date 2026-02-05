targetScope = 'resourceGroup'

@description('The Azure region for resources')
param location string = resourceGroup().location

@description('Environment name')
param environment string = 'dev'

@description('Project name')
param projectName string = 'lunchvote'

@description('Common tags for all resources')
param tags object = {
  Environment: 'dev'
  Project: 'LunchVoteApp'
  ManagedBy: 'Bicep'
}

// Reference modules here
// module resourceGroup '../../modules/resource-group.bicep' = {
//   name: 'resourceGroupDeployment'
//   params: {
//     location: location
//     environment: environment
//     projectName: projectName
//     tags: tags
//   }
// }

// Outputs
// output resourceGroupName string = resourceGroup.outputs.name
