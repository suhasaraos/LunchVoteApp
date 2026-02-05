# Terraform Infrastructure

## Structure

- `environments/` - Environment-specific configurations (dev, staging, prod)
- `modules/` - Reusable Terraform modules
- `shared/` - Shared variables and configurations

## Usage

### Initialize Terraform
```bash
cd environments/dev
terraform init
```

### Plan Changes
```bash
terraform plan -var-file="terraform.tfvars"
```

### Apply Changes
```bash
terraform apply -var-file="terraform.tfvars"
```

## Best Practices

1. Use remote state (Azure Storage, Terraform Cloud, etc.)
2. Use workspaces for environment isolation
3. Keep modules generic and reusable
4. Use variables for all configurable values
5. Version your modules
