# Bicep Infrastructure

## Structure

- `environments/` - Environment-specific deployments (dev, staging, prod)
- `modules/` - Reusable Bicep modules
- `shared/` - Shared parameter files and configurations

## Usage

### Build Bicep to ARM Template
```bash
az bicep build --file main.bicep
```

### Deploy to Azure
```bash
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters @parameters.dev.json
```

### Validate Deployment
```bash
az deployment group validate \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters @parameters.dev.json
```

## Best Practices

1. Use modules for reusable components
2. Parameterize all configurable values
3. Use parameter files for environment-specific values
4. Add descriptions to all parameters
5. Use resource symbolic names consistently
6. Leverage Bicep's type safety
