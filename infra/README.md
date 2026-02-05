# Infrastructure as Code

This directory contains all Infrastructure as Code (IaC) for the LunchVoteApp project.

## Structure

```
infra/
├── terraform/     # Terraform configurations
├── bicep/         # Bicep configurations
└── README.md      # This file
```

## Choosing Between Terraform and Bicep

- **Terraform**: Multi-cloud support, larger ecosystem, HashiCorp Configuration Language (HCL)
- **Bicep**: Azure-native, simpler syntax, tight integration with Azure Resource Manager

## Quick Start

### Terraform
```bash
cd terraform/environments/dev
terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply
```

### Bicep
```bash
cd bicep/environments/dev
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters @parameters.dev.json
```

## Best Practices

1. Use environment-specific configurations
2. Leverage modules for reusability
3. Keep secrets in Azure Key Vault, not in code
4. Use remote state management (Terraform) or deployment history (Bicep)
5. Tag all resources consistently
6. Document all infrastructure changes
