# Infrastructure as Code

This directory contains all Infrastructure as Code (IaC) for the LunchVoteApp project.

## Structure

```
infra/
├── README.md          # This file
├── terraform/         # Terraform IaC
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   ├── terraform.tfvars.example
│   └── modules/
└── bicep/             # Bicep IaC
    ├── main.bicep
    ├── parameters.dev.json
    └── modules/
```

## Quick Start

### Terraform
```bash
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform plan
terraform apply
```

### Bicep
```bash
cd bicep
# Edit parameters.dev.json with your values
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file main.bicep \
  --parameters parameters.dev.json
```

## Choosing Between Terraform and Bicep

- **Terraform**: Use if you need multi-cloud support or prefer HCL syntax
- **Bicep**: Use if you're Azure-only and want native ARM integration

Both implement the same infrastructure.
