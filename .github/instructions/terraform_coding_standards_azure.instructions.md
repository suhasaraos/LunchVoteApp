---
applyTo: 'infra/terraform/**'
---

# Terraform Module Coding Standards (Azure)

This document provides coding standards and best practices for developing Terraform modules for Azure at Suncorp. Follow these guidelines when generating Terraform code.

## Supported Versions

### Terraform Binary

* We currently utilize `1.11.4` for Terraform version constraints in our modules, which is the latest stable release as of June 2024 and includes important features and improvements while maintaining compatibility with existing code.

```hcl
terraform {
  required_version = "1.11.4"
}
```

### Azure Provider

* We currently utilize `~> 4.0` for Azure provider version constraints in our modules, which allows for non-breaking updates and new features while avoiding breaking changes that may come with major version releases.

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

## General Coding Standards

* Always **lower case**, always **snake_case**. Capitalize only when required and correct to do so, including free text comments and headings.
  * In comments and documentation, use standard English capitalization:
    * "This creates an Azure Storage Account" ✓
    * "this creates an azure storage account" ✗
  * Code identifiers within comments should maintain snake_case (e.g., "The `storage_account_primary` resource connects to...")
* Use [Terraform string functions](https://developer.hashicorp.com/terraform/language/functions) to translate **snake_case** to other naming conventions that may be used elsewhere.
* When **snake_case** is not available, use **PascalCase**.
* Indents in multiples of two whitespace characters
* Resource, variable, output, etc. names should be structured as follows:
  * Narrowing scope e.g. `storage_endpoints_blob` as opposed to `blob_storage_endpoints`
  * noun_verb format e.g. `role_assignments_approved` as opposed to `approved_role_assignments`
* Code should be "concisely verbose" - descriptive enough to be self-documenting, but not unnecessarily long.
  * Only abbreviate words when there can be no ambiguity. Widely accepted acronyms (e.g. `sql` instead of `structured_query_language`) can be used but must be in lowercase.
  * Examples of concisely verbose naming:
    * **Good:** `storage_account_primary`, `sql_database_main`, `virtual_network_hub`
    * **Too concise:** `sa`, `db`, `vnet`
* Always include comments when readers will benefit from them, such as when complex logic is being used or an uncommon/unusual attribute value is enforced or set as default.
* Always include README.md and CHANGELOG.md files with your code.
  * Use [terraform-docs](https://github.com/terraform-docs/terraform-docs) to dynamically generate README sections for your Terraform modules.
* Format your code with **`terraform fmt -recursive`**
* **Provider Configuration Rules:**
  * Provider configuration blocks (`provider "azurerm" {}`) should **only** be declared in Configuration modules (the entry point).
  * Core and Pattern modules should **never** contain provider blocks - they inherit providers from the calling Configuration module.
  * All modules (Core, Pattern, and Configuration) **must** define `required_providers` in the terraform block to specify version constraints.
  * If a module provisions resources across multiple regions or subscriptions, it must:
    * Define provider aliases in the module's versions.tf
    * Document the required provider aliases in README
    * Accept providers via the `providers` map in module calls

Example for a multi-region module in `versions.tf`:

```hcl
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
      configuration_aliases = [azurerm.primary, azurerm.secondary]
    }
  }
}
```

Then in the calling configuration:

```hcl
module "multi_region" {
  source = "../terraform-azurerm-multi-region-module"

  providers = {
    azurerm.primary   = azurerm.australiaeast
    azurerm.secondary = azurerm.australiasoutheast
  }

  name = var.name
}
```

* **Hardcoding Rules:**
  * **NEVER** hardcode subscription IDs or region names directly in resource blocks - use data sources and variables instead.
  * **Exception:** Hardcoded values **ARE ALLOWED** when:
    * Defining default values in variable blocks
    * Calling modules (passing literal values to module inputs)
    * In local values that transform or compose other values

**Example - Bad (hardcoded in resource):**

```hcl
resource "azurerm_policy_assignment" "diagnostics" {
  name                 = "${var.name}-diagnostics"
  scope                = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-hardcoded" # Hardcoded - BAD
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/abcd1234-abcd-1234-abcd-abcdef123456"

  parameters = jsonencode({
    logAnalyticsWorkspaceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourcegroups/rg-hardcoded/providers/microsoft.operationalinsights/workspaces/law-hardcoded"
  })
}
```

**Example - Good (using data source):**

```hcl
data "azurerm_subscription" "current" {}

data "azurerm_log_analytics_workspace" "diagnostics" {
  name                = var.log_analytics_workspace_name
  resource_group_name = var.log_analytics_resource_group_name
}

resource "azurerm_policy_assignment" "diagnostics" {
  name                 = "${var.name}-diagnostics"
  scope                = data.azurerm_subscription.current.id
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/abcd1234-abcd-1234-abcd-abcdef123456"

  parameters = jsonencode({
    logAnalyticsWorkspaceId = data.azurerm_log_analytics_workspace.diagnostics.id
  })
}
```

**Example - Acceptable (in module block):**

```hcl
module "virtual_network" {
  source = "git::ssh://bitbucket.int.corp.sun:2222/tf/terraform-azurerm-virtual-network.git?ref=3.LATEST"

  name          = var.name
  env           = var.env
  address_space = ["10.10.0.0/16"]         # Hardcoded in module call - ACCEPTABLE
  location      = "australiaeast"          # Hardcoded in module call - ACCEPTABLE
}
```

## Files and File Names

* File names in **snake_case**
* One file per group of declared resources e.g.: sql_database.tf, storage_account.tf, virtual_network.tf, variables.tf, locals.tf, provider.tf, versions.tf
* No trailing spaces on lines
* Files conclude with a trailing newline (reduce git warnings and confusing git diffs when appending content)

## Data, Resources, and Modules

* Terraform resources declared in **snake_case**
* Name arguments of resources being provisioned should be **kebab-case**
* In many cases, `name` and `location` attributes are primary attributes, with only one or two other attributes required. In these cases, sort attributes logically, by order of importance/relevance

Sort generic resources' attributes logically:

```hcl
resource "azurerm_resource_type" "an_item_name" {
  name                = "item-name-${var.env}"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  # ... other important attributes
}
```

* For items with large numbers of attributes, group attributes into sections based on the service or functionality they apply to (e.g., networking, security, monitoring), then sort attributes alphabetically within each group. Use sectional heading comments to separate groups:

```hcl
resource "azurerm_linux_virtual_machine" "example" {
  name                = "vm-${var.name}-${var.env}"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  size                = var.vm_size

  ## Networking configuration ##
  network_interface_ids = [
    azurerm_network_interface.main.id,
  ]

  ## OS configuration ##
  admin_username                  = var.admin_username
  disable_password_authentication = true

  admin_ssh_key {
    public_key = var.admin_ssh_public_key
    username   = var.admin_username
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Premium_LRS"
  }

  source_image_reference {
    offer     = "0001-com-ubuntu-server-focal"
    publisher = "Canonical"
    sku       = "20_04-lts-gen2"
    version   = "latest"
  }

  ## Identity configuration ##
  identity {
    type = "SystemAssigned"
  }

  ## Tags ##
  tags = local.common_tags
}
```

* Meta-arguments must be listed at the top of resource and module blocks in the following order:
  * `source` (modules only - always first line after opening brace)
  * `depends_on`
  * `for_each` or `count`
  * `providers`
  * All other resource/module-specific arguments

Example for modules:

```hcl
module "example" {
  source = "git::ssh://bitbucket.int.corp.sun:2222/tf/terraform-azurerm-example.git?ref=1.LATEST"

  depends_on = [module.xyz]
  count = var.create_example ? 1 : 0

  providers = {
    azurerm = azurerm.alternate
  }

  name     = var.name
  location = var.location
}
```

Example for resources:

```hcl
resource "azurerm_storage_account" "example" {
  depends_on = [azurerm_key_vault.example]

  count = var.create_storage ? 1 : 0

  name                     = "st${var.name}${var.env}"
  location                 = var.location
  resource_group_name      = azurerm_resource_group.main.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
}
```

* use `for_each` instead of `count` whenever possible
* Never create indexed items (with `count`) when a named item (with `for_each`) could be used instead.
* `count` on resources should only be considered if you want to create one or none of an item.
* use `for_each` instead of multiple definition blocks for related items
* only use `depends_on` when an explicit dependency is required, and always include a comment as to why the explicit dependency exists
* Avoid repeating names in type and instance declarations (be "concisely verbose")
* use `data "azurerm_policy_definition" "item_name"` instead of `data "azurerm_policy_definition" "item_name_policy"`

## Tag Management

**IMPORTANT:** Unlike AWS, the Azure provider does NOT support `default_tags` in the provider block.

Tags should be managed using a `common_tags` local value and applied to resources explicitly:

**Example tag management pattern:**

```hcl
# In locals.tf
locals {
  common_tags = {
    Environment = var.env
    ManagedBy   = "Terraform"
    Project     = var.name
    CostCenter  = var.cost_center
  }
}

# Apply to resources
resource "azurerm_resource_group" "main" {
  name     = "rg-${var.name}-${var.env}"
  location = var.location
  tags     = local.common_tags
}

# Merge with resource-specific tags
resource "azurerm_storage_account" "data" {
  name                     = "st${var.name}data${var.env}"
  location                 = var.location
  resource_group_name      = azurerm_resource_group.main.name
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = merge(
    local.common_tags,
    {
      Purpose = "Data Storage"
      Backup  = "Daily"
    }
  )
}
```

**Benefits:**

* Centralized tag management in locals.tf
* Easy to maintain and update tags
* Can merge common tags with resource-specific tags
* Consistent tagging across infrastructure

> NOTE: Do NOT use lifecycle ignore_changes for tags. Always manage tags explicitly through locals.

## Strings

In most cases, optimal readability of string concatenation is achieved by combining raw text with string interpolation, as follows:

```hcl
"${var.name}-free-text-${lower(var.a_string)}"
```

> NOTE: Terraform also offers a powerful [`format`](https://developer.hashicorp.com/terraform/language/functions/format) function, but readability is sacrificed when it is used for general string concatenation. Please reserve `format` for specific cases where complex string formatting and interpolation based on [conditional statements](https://developer.hashicorp.com/terraform/language/expressions/conditionals) is required.

## Variables

All `variable` blocks must contain a `description` and `type` at a minimum.

### Inputs (variables.tf)

```hcl
variable "my_variable" {
  description = "A clear description of what this variable is for"
  type        = string
  default     = "default-value"
}
```

**Guidelines for variable defaults:**

* **Provide defaults** when defining patterns with sensible values that work in most cases but can be overridden:

```hcl
variable "vm_size" {
  description = "The size of the virtual machine"
  type        = string
  default     = "Standard_B2s"
}

variable "env" {
  description = "The environment descriptor"
  type        = string
  default     = "nonprod"
}
```

* **Do not provide defaults** for environment-specific or security-sensitive values:

```hcl
variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  # No default - must be provided
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  # No default - must be explicitly chosen
}

variable "database_password" {
  description = "Password for database administrator"
  type        = string
  sensitive   = true
  # No default - must be securely provided
}
```

### Locals (locals.tf)

```hcl
locals {
  common_tags = {
    Environment = var.env
    ManagedBy   = "public-cloud"
  }

  resource_suffix = "${var.name}-${var.env}"
}
```

* Use `map(object({ ... }))` instead of `set(object({ ... }))` whenever possible, keying each object with a unique identifier.
* Use `set` instead of `list` unless the target API explicitly requires an ordered list/array to be passed to it.

Use locals to:

* **Avoid repetition** - create reusable values used across multiple resources:

```hcl
locals {
  resource_suffix = "${var.name}-${var.env}"
}

resource "azurerm_storage_account" "data" {
  name                     = "st${replace(local.resource_suffix, "-", "")}data"
  location                 = var.location
  resource_group_name      = azurerm_resource_group.main.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_account" "logs" {
  name                     = "st${replace(local.resource_suffix, "-", "")}logs"
  location                 = var.location
  resource_group_name      = azurerm_resource_group.main.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
}
```

* **Transform data** - process complex inputs for use in resources:

```hcl
locals {
  subnet_cidrs = cidrsubnets(var.vnet_address_space, 8, 8, 8)

  # Convert list of CIDRs into a stable key, value map for for_each
  subnet_map = {
    for idx, cidr in local.subnet_cidrs :
    "subnet-${idx}" => cidr
  }
}

resource "azurerm_subnet" "example" {
  for_each = local.subnet_map

  name                 = each.key
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [each.value]
}
```

### Common Variable Name/Value Standards

| Variable Name | Description | Allowed Values (default in bold) |
| :--- | :--- | :--- |
| name | A unique name to assign to resources created by this module | \<a-kebab-case-string\> |
| env | The environment descriptor into which resources created by this module will be provisioned | prod, **nonprod**, sandpit |
| location | Azure region for resources | **australiaeast**, australiasoutheast, eastus, etc. |

#### Common Variable Examples in Code

```hcl
variable "name" {
  description = "A unique name to assign to resources created by this module"
  type        = string
}
```

```hcl
variable "env" {
  description = "The environment descriptor into which resources created by this module will be provisioned"
  type        = string
  default     = "nonprod"
  validation {
    condition     = contains(["nonprod", "prod", "sandpit"], var.env)
    error_message = "The env value must be one of 'nonprod', 'prod', or 'sandpit'"
  }
}
```

```hcl
variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "australiaeast"
}
```

## Sensitive Variables

**ALWAYS** use `sensitive = true` for variables containing secrets, passwords, API keys, or other confidential data to prevent them from being displayed in Terraform output.

**Example:**

```hcl
variable "database_password" {
  description = "Password for database administrator"
  type        = string
  sensitive   = true
}

variable "api_key" {
  description = "API key for external service"
  type        = string
  sensitive   = true
}

variable "private_key" {
  description = "Private key for SSH access"
  type        = string
  sensitive   = true
}
```

**Rationale:** Marking variables as sensitive prevents their values from appearing in CLI output, logs, or plan files, reducing the risk of accidental exposure of confidential information.

## Comments

### Multi-line

```hcl
# This is a multi-line comment.
# This structure (same as single-line, repeated) has been chosen with simplicity and convenience in mind.
# Modern-day IDEs like VS Code offer keyboard shortcuts (such as Ctrl + /) that insert a
# language-specific comment symbol at the start of each selected / highlighted line
```

### Single Line, Inline

```hcl
# This is a comment.
<some_code_here> # and a trailing, inline comment
```

## Sectional Headings

### Major

```hcl
###############
# Major heading
###############
code continues immediately on next line
```

### Minor

```hcl
## Minor heading ##
code continues immediately on next line
```

## Module Types

There are three main module types, forming a hierarchy:

| Module Type            | Description                                                                 | .gitignore | terraform-docs | Provider Configuration                | Terraform Block                |
|----------------------- |-----------------------------------------------------------------------------|------------|---------------|----------------------------------------|-------------------------------|
| **Core Module**        | Building block, maps to a single Azure service (e.g., Storage Account, SQL Database). Reusable, not environment-specific. Called by Pattern modules. | No         | Yes           | No (inherits from caller)              | Define `required_providers` with version constraints           |
| **Pattern Module**     | Calls Core module(s) and defines resources to build a solution. Called by Configuration modules. | Yes         | Yes           | No (inherits from caller)              | Define `required_providers` with version constraints           |
| **Configuration Module** | Root/entry point, configures a solution for an environment (e.g., `<pattern-module-name>-nonprod`). Calls Pattern module. | Yes        | Yes (simple README describing solution)           | Yes (entry point for provider config)  | Define `terraform` version constraint and backend configuration                   |

**Applies to All Modules:**

* For the first Changelog entry (v1.0.0), copy the first commit hash on the master branch and construct a URL in this format:

  ```markdown
  ## [1.0.0](https://bitbucket.int.corp.sun/projects/<project_name>/repos/<repo_name>/compare/diff?sourceBranch=refs/tags/1.0.0&targetBranch=<first_commit_on_master_branch>) - YYYY-MM-DD
  ```

* Subsequent changelog entries should link to the compare/diff view between versions
* Date format: ISO 8601 (YYYY-MM-DD)

## Module Architecture

### Core Module

Core modules for Azure services define the data sources and resources to provision and configure specific Azure services, including prerequisites the service relies on. Examples:

* Storage Account module
* SQL Database module
* Virtual Network module
* Key Vault module

### Pattern Module

Pattern modules define a solution by combining Core modules and additional resources. For example, a Pattern Module for a web application could contain the following:

* Storage Account Core module (for static assets)
* SQL Database Core module (for application data)
* App Service Core module (for web hosting)
* Key Vault Core module (for secrets)
* Additional resources: Application Insights, DNS records, etc.

### Configuration Module

The configuration deploys to a certain location/environment, containing:

* Environment specifics - Provider and Terraform configuration:
  * Azure Storage backend (resource group, storage account, container, state file name)
  * Provider features and configuration
* Inputs to the pattern module
* Environment-specific variables (nonprod, prod, sandpit)

## Documentation Standards

### README.md

A standardized `README.md` file allows end users to find what they are looking for and absorb general information about the code in an efficient manner.

#### Core and Pattern Module READMEs

* The first line of a core or pattern module `README.md` should always be a markdown heading (e.g., `# Azure Storage Account Terraform Core Module`) to clearly indicate the purpose of the document.
* It should indicate which High-Level Design (HLD) document it follows (if applicable)
* It should include a basic usage example which details the minimum required variables and provider configuration to use the module.
* It should include a complex usage example which details all possible variable inputs to use the module.
* Use terraform-docs to dynamically generate comprehensive documentation sections within the README file, based on the terraform code. These READMEs should provide in-depth explanations of the module's purpose, usage, inputs, outputs, and examples.
* All headings and subheadings should be in sentence case (e.g., "Usage" instead of "USAGE") and use markdown heading syntax (e.g., `## Usage`).

terraform-docs usage:

```sh
terraform-docs markdown .
```

#### Configuration Module READMEs

* The first line of a configuration module `README.md` should always be a markdown heading (e.g., `# LunchVote App Nonprod Configuration`) to clearly indicate the purpose of the document.

Configuration modules require a simpler README that briefly explains:

* What the configuration is deploying
* Which High-Level Design (HLD) document it follows (if applicable)
* Prerequisites for deployment (Azure CLI, Terraform version, backend storage)
* Backend configuration details and setup instructions
* Basic deployment instructions
* Outputs and post-deployment steps
* Contact information for support

Configuration module READMEs should be manually written and focus on deployment context rather than technical implementation details (which are documented in the Pattern/Core modules being called).

### CHANGELOG.md

This file includes notes relating to the changes to the code repository over time.

* The first line of the CHANGELOG.md file should always be `# Change Log` to clearly indicate the purpose of the document.
* The CHANGELOG.md file must be updated prior to every release being merged to the _master_ branch.
* Each change in the log is headed by the semantic version (major.minor.patch) of the change as well as a link to the diff between the changed code and its predecessor.
* Every change can be broken down into one or more Features or Fixes
* Changes that "break" the way the code works or can be interacted with must have the relevant feature(s) or fix(es) flagged as **BREAKING** changes.
* All entries must be listed with a link to the Bitbucket/GitHub compare view between the current and previous version tags.
* All entry headings must be dated in ISO 8601 format (**YYYY-MM-DD**).
* All headings and subheadings should be in sentence case (e.g., "Features" instead of "FEATURES") and use markdown heading syntax (e.g., `## Features`).

#### CHANGELOG.md Example

```markdown
# Change Log

## [3.1.4](https://bitbucket.int.corp.sun/projects/CS/repos/my-repo-name/compare/diff?sourceBranch=refs/tags/3.1.4&targetBranch=refs/tags/3.1.3) 2026-02-16

Features:

* Added support for customer-managed encryption keys on Storage Accounts
* **BREAKING** Changed variable name from `replication_type` to `account_replication_type` to match Azure provider naming

Fixes:

* Fixed issue where tags were not being applied to subnet resources
* Improved validation on location variable to prevent invalid Azure regions

## [3.1.3](https://bitbucket.int.corp.sun/projects/CS/repos/my-repo-name/compare/diff?sourceBranch=refs/tags/3.1.3&targetBranch=refs/tags/3.1.2) 2026-01-15

Feature:

* Initial support for Azure Policy assignments

Fix:

* Fixed provider version constraint to allow patch updates
```

### Markdown Conventions

* Use a single blank line around each section of the document or change in Markdown structure (e.g. before and after a heading, list, code block)
* Always increment/decrement heading level by one hash to avoid linting warnings and errors
* No two headings should have the same name/content
* Use `**<text>**` for bold
* Use `*<text>*` for italics
* Use `` `<text>` `` for inline code
* Use triple backticks with language descriptor for multiline code blocks (e.g. ```hcl) and close with triple backticks on a new line
* Use `[link text](https://address.to.link.to)` for URLs
* Use `1.` for ordered lists (Markdown will number correctly)
* Use `*` for unordered lists
* Use indents in multiples of four characters to maintain list ordering

> NOTE: Markdown files use 4-space indentation for nested lists (following Markdown conventions), while HCL/Terraform files use 2-space indentation (following Terraform conventions). This difference is intentional and follows each format's best practices.

## Azure-Specific Best Practices

### Resource Naming

Azure has specific naming rules and length limits for each resource type. Follow these guidelines:

* Storage Account names: 3-24 characters, lowercase letters and numbers only
* Key Vault names: 3-24 characters, alphanumeric and hyphens
* Resource Group names: 1-90 characters, alphanumeric, underscores, parentheses, periods, and hyphens
* SQL Server names: 1-63 characters, lowercase letters, numbers, and hyphens

Use string functions to ensure compliance:

```hcl
resource "azurerm_storage_account" "example" {
  name = lower(replace("st${var.name}${var.env}", "-", ""))  # Remove hyphens, make lowercase
  # ... other attributes
}
```

### Managed Identities

Prefer system-assigned managed identities for Azure resources when possible:

```hcl
resource "azurerm_app_service" "example" {
  # ... other attributes

  identity {
    type = "SystemAssigned"
  }
}
```

### RBAC vs Access Policies

* For Key Vault: Use RBAC authorization (`rbac_authorization_enabled = true`) instead of access policies
* For Storage Accounts: Use RBAC role assignments instead of shared key access
* Always use role assignments for granting permissions between resources

```hcl
resource "azurerm_role_assignment" "example" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_app_service.main.identity[0].principal_id
}
```

### Backend State Configuration

Azure Storage backend example for Configuration modules:

```hcl
terraform {
  required_version = "1.11.4"

  backend "azurerm" {
    resource_group_name  = "<resource-group-name>"
    storage_account_name = "<storage-account-name>"
    container_name       = "<container-name>"
    key                  = "<state-file-name>.tfstate"
  }
}
```

**Azure backend features (automatic):**
* Encryption at rest (always enabled on Azure Storage)
* State locking via blob lease mechanism
* No additional configuration needed (unlike AWS S3 which requires DynamoDB table for locking)

### Resource Groups

Every Azure resource requires a resource group. Best practices:

* Create resource group in Configuration module
* Pass resource group name to all modules
* Use consistent naming: `rg-<name>-<env>`

```hcl
resource "azurerm_resource_group" "main" {
  name     = "rg-${var.name}-${var.env}"
  location = var.location
  tags     = local.common_tags
}
```
