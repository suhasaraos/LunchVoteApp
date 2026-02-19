---
applyTo: 'infra/terraform/**'
---

# Module Generation Guide (Azure)

Step-by-step procedural guide for generating complete Terraform modules for Azure. This guide focuses on the workflow and process. For detailed coding standards, naming conventions, and formatting rules, refer to terraform_coding_standards_azure.instructions.md.

## CRITICAL: Module Type Selection

**ALWAYS ask the user which module type they want to create if not explicitly specified.**

When a user asks to "create a module" or "generate a module" without specifying the type, you MUST prompt them with:

"Which type of module would you like to create?
1. **Core Module** - Single Azure service (e.g., Storage Account, SQL Database, Virtual Network)
2. **Pattern Module** - Combination of Core modules for a solution
3. **Configuration Module** - Environment-specific deployment

Please specify which type you need.

## Azure-Specific Considerations During Module Generation

1. **Resource Naming:** Azure has specific naming rules and length limits per resource type
2. **Location vs Region:** Use `location` variable name (not `aws_region`)
3. **Resource Groups:** Every resource needs a resource group - typically created in Configuration module
4. **Managed Identities:** Use system-assigned or user-assigned managed identities for authentication
5. **RBAC:** Use role assignments for permissions (not IAM policies)
6. **Backend State:** Azure blob storage with automatic encryption and lease-based locking
7. **Tags:** Apply via locals, not provider default_tags (Azure limitation)

## Core Module Generation Steps

### 1. Create versions.tf

For a Core module, define required providers. Terraform version constraint is not defined in core modules. See terraform_coding_standards_azure.instructions.md for version requirements.

```hcl
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}
```

### 2. Create variables.tf

Include at minimum:
* `name` variable (no default)
* `env` variable with validation (default: "nonprod")
* Service-specific variables with clear descriptions
* Mark sensitive variables appropriately (see Sensitive Variables section)
* Create validation blocks specific to the service (e.g., allowed values, regex patterns)

See terraform_coding_standards_azure.instructions.md for detailed guidelines on variable definitions and validation.

### 3. Create locals.tf

Use for repeated values, data transformations, complex logic, and common tags. See Locals section in terraform_coding_standards_azure.instructions.md for detailed guidelines.

```hcl
locals {
  common_tags = {
    Environment = var.env
    ManagedBy   = "Terraform"
    Project     = var.name
  }
}
```

### 4. Create service-specific .tf files

Name after the service (e.g., `storage_account.tf`, `sql_database.tf`, `virtual_network.tf`) using snake_case. Follow meta-argument ordering and attribute sorting rules from terraform_coding_standards_azure.instructions.md.

### 5. Create outputs.tf

Export useful values (resource IDs, endpoints, connection strings). Use noun_verb format as defined in terraform_coding_standards_azure.instructions.md.

### 6. Generate README.md

Run `terraform-docs markdown .` then edit to add or modify sections as needed. Include:
* Module purpose
* Usage examples (minimum and complex)
* Prerequisites
* Notes

See Documentation section in terraform_coding_standards_azure.instructions.md for detailed guidelines on README content and formatting.

### 7. Create CHANGELOG.md

Create initial entry following the format and link structure defined in terraform_coding_standards_azure.instructions.md.

```markdown
# Change Log

## [1.0.0](https://bitbucket.int.corp.sun/projects/<project_name>/repos/<module_repository>/compare/diff?sourceBranch=refs/tags/1.0.0&targetBranch=<first_commit_on_master_branch>) - YYYY-MM-DD

Features:

* Initial release of [Module Name] module
```

## Pattern Module Generation Steps

### 1. Create versions.tf

Same as the Core module, but may include multiple providers if needed (see Provider Configuration Rules in terraform_coding_standards_azure.instructions.md).

### 2. Create variables.tf

Include standard `name` and `env` variables plus variables for each Core module being called. Group related variables with comments where applicable.

### 3. Create locals.tf

Transform input variables for Core module consumption.

### 4. Create module calls

In `main.tf` or service-specific files, call Core modules using the git source format with version references. Follow meta-argument ordering. Use `depends_on` only when explicit dependency is required.

```hcl
module "storage_account" {
  source = "git::ssh://bitbucket.int.corp.sun:2222/tf/terraform-azurerm-storage-account.git?ref=1.LATEST"

  name = var.name
  env  = var.env
  # ... other variables
}
```

### 5. Create outputs.tf

Export values from Core modules that consumers need.

### 6. Generate README.md and CHANGELOG.md

Same process as a Core module generation.

## Configuration Module Generation Steps

### 1. Create versions.tf

Configuration modules do define the Terraform version constraint (currently 1.11.4) and Azure Storage backend configuration:

```hcl
terraform {
  required_version = "1.11.4"

  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "sttfstate<project>"
    container_name       = "tfstate"
    key                  = "<project>-<env>.tfstate"
  }
}
```

> NOTE: Azure Storage backend automatically provides:
* Encryption at rest (always enabled)
* State locking via blob lease mechanism
* No additional parameters needed (unlike AWS S3)

### 2. Create provider.tf

Define Azure provider with region. This is the only module type that should contain provider configuration.

```hcl
provider "azurerm" {
  features {
    # Azure-specific feature flags
  }
}
```

**Tag Management:** Unlike AWS, Azure provider does not support `default_tags` block. Use locals for common tags:

```hcl
# In locals.tf
locals {
  common_tags = {
    AppSpace    = "A-XXXXXX"
    Environment = var.env
    ManagedBy   = "public-cloud"
    SourcePath  = "<git_project_name>/<git_repository_name>/"
  }
}

# Apply to resources as needed
resource "azurerm_resource_group" "main" {
  name     = "rg-${var.name}-${var.env}"
  location = var.location
  tags     = local.common_tags
}
```

### 3. Create variables.tf

Include environment-specific variables:
* `location` (default: "australiaeast")
* `env` (default: "nonprod")
* Variables to pass to Pattern module

```hcl
variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "australiaeast"
}
```

### 4. Create main.tf

Single Pattern module call using git source with version reference.

### 5. Create outputs.tf

Pass through important outputs from Pattern module.

### 6. Create .gitignore

Add standard Terraform .gitignore entries:

```gitignore
# Terraform
.terraform/
.terraform.lock.hcl
*.tfstate
*.tfstate.*
*.tfstate.backup
*.tfvars
terraform.tfplan
.terraformrc
terraform.rc
override.tf
override.tf.json
*_override.tf
*_override.tf.json
crash.log
crash.*.log

# Sensitive files
**/.terraform/*
```

### 7. Create simple README.md

Manually write deployment-focused documentation (not technical implementation details). Include:
* What is being deployed
* Prerequisites (Azure CLI, Terraform version, backend storage)
* Backend configuration instructions
* Deployment steps
* Contact information

### 8. Create CHANGELOG.md

Same process as Core and Pattern module generation. Follow the format defined in terraform_coding_standards_azure.instructions.md.

## Final Validation Checklist

Before committing:

* [ ] Run `terraform init -backend=false` (or with backend if configured)
* [ ] Run `terraform validate`
* [ ] Run `terraform-docs markdown .` (for Core/Pattern modules)
* [ ] Check all files end with newline
* [ ] No trailing whitespace
* [ ] All variables have descriptions and types
* [ ] Sensitive variables marked appropriately
* [ ] No hardcoded subscription IDs or regions in resources
* [ ] Meta-arguments in correct order
* [ ] Comments added for complex logic
* [ ] CHANGELOG.md is updated with actual commit hash
* [ ] README.md is complete with module usage examples and terraform-docs output
* [ ] Run `terraform fmt -recursive`
* [ ] Tags managed via locals.tf (not provider default_tags)
* [ ] For Pattern & Core modules, cleanup files related to terraform init (e.g. .terraform, .terraform.lock.hcl)
