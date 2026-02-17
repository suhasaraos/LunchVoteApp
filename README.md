# Lunch Vote App

A team-based lunch voting application that enables groups within an organization to democratically decide on lunch options.

## Project Structure

```
LunchVoteApp/
├── src/
│   ├── LunchVoteApi/          # .NET 10 Web API
│   └── lunch-vote-spa/        # React + Vite + TypeScript SPA
├── tests/
│   └── LunchVoteApi.Tests/    # Backend unit & integration tests
├── infra/
│   ├── bicep/                 # Azure Bicep IaC templates
│   └── terraform/             # Terraform IaC templates
└── LunchVoteApp.sln           # Visual Studio solution file
```

## Technology Stack

### Backend
- .NET 10 Web API
- Entity Framework Core 10
- Azure SQL Database

### Frontend
- React 18+
- Vite 5+
- TypeScript 5+

### Infrastructure
- Azure App Service (Backend API + Frontend SPA)
- Azure SQL Database
- Azure Key Vault

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker (optional, for local SQL Server)
- Azure CLI
- Bicep CLI or Terraform (for infrastructure deployment)

## Environment Setup

### .NET SDK Installation

Check your current .NET version:

```powershell
dotnet --version
```

List installed SDKs:

```powershell
dotnet --list-sdks
```

If you need to install .NET 10.0, use the official installer script:

```powershell
# Download the installer script
Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1

# Install .NET 10.0
.\dotnet-install.ps1 -Channel 10.0 -InstallDir "$env:USERPROFILE\.dotnet"

# Add to PATH (current session)
$env:PATH="$env:USERPROFILE\.dotnet;$env:PATH"

# Verify installation
dotnet --version
```

### Node.js Installation

To install Node.js v20.11.1 manually on Windows:

```powershell
# Download Node.js
Invoke-WebRequest -Uri "https://nodejs.org/dist/v20.11.1/node-v20.11.1-win-x64.zip" -OutFile "$env:USERPROFILE\Downloads\node.zip"

# Extract to .nodejs folder
Expand-Archive -Path "$env:USERPROFILE\Downloads\node.zip" -DestinationPath "$env:USERPROFILE\.nodejs" -Force

# Move files to parent directory
Move-Item "$env:USERPROFILE\.nodejs\node-v20.11.1-win-x64\*" "$env:USERPROFILE\.nodejs\"

# Add to PATH (current session)
$env:PATH="$env:USERPROFILE\.nodejs;$env:PATH"
```

### Terraform Installation

This project uses **Terraform v1.11.4**.

**Windows Installation:**

Download and install Terraform v1.11.4:

```powershell
# Download Terraform for Windows
Invoke-WebRequest -Uri "https://releases.hashicorp.com/terraform/1.11.4/terraform_1.11.4_windows_amd64.zip" -OutFile "$env:USERPROFILE\Downloads\terraform.zip"

# Extract to a local directory (e.g., C:\terraform)
Expand-Archive -Path "$env:USERPROFILE\Downloads\terraform.zip" -DestinationPath "C:\terraform" -Force

# Add to PATH (current session)
# PowerShell:
$env:PATH="C:\terraform;$env:PATH"

# CMD:
# set PATH=C:\terraform;%PATH%
```

**Other Operating Systems:**

Follow the official installation guide: https://developer.hashicorp.com/terraform/install

**Verify Installation:**

```powershell
terraform -version
```

Expected output: `Terraform v1.11.4`

### Azure CLI

Verify Azure CLI installation:

```powershell
az version
```

### Important Notes

- **Resource Naming**: Terraform resources include a random 6-character suffix to ensure global uniqueness (e.g., `app-lunchvote-api-dev-a1b2c3`).

## Local Development

### Start SQL Server (Docker)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-lunchvote \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### Start Backend API

```bash
cd src/LunchVoteApi
dotnet run
```

The API will be available at:
- `https://localhost:52544` (HTTPS)
- `http://localhost:52545` (HTTP)

### Start Frontend

```bash
cd src/lunch-vote-spa
npm install
npm run dev
```

The SPA will be available at `http://localhost:5173`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/polls` | Create a new poll |
| GET | `/api/polls/active?groupId={groupId}` | Get active poll for a group |
| GET | `/api/polls/{pollId}/results` | Get poll results |
| POST | `/api/votes` | Submit a vote |
| GET | `/api/groups` | Get all group IDs with active polls |

## Testing

### Backend Tests

```bash
cd tests/LunchVoteApi.Tests
dotnet test
```

## Deployment

You can deploy the infrastructure using either **Bicep** or **Terraform**.

### Option 1: Deploy with Bicep

#### Deploy Infrastructure

```bash
# Login to Azure
az login

# Get your credentials
az ad signed-in-user show --query id -o tsv          # Object ID
az ad signed-in-user show --query userPrincipalName -o tsv  # Email

# Edit parameters
code infra/bicep/parameters.dev.json

# Create resource group
az group create --name rg-lunchvote-dev --location australiaeast

# Deploy Bicep templates
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file infra/bicep/main.bicep \
  --parameters infra/bicep/parameters.dev.json
```

#### Bicep Resources Created

- Resource Group: `rg-lunchvote-dev`
- Backend App Service Plan: `plan-lunchvote-dev` (Linux, B1 SKU)
- Backend App Service: `app-lunchvote-api-dev` (.NET 10.0)
- Frontend App Service Plan: `plan-lunchvote-spa-dev` (Linux, B1 SKU)
- Frontend App Service: `app-lunchvote-spa-dev` (Node.js 20 LTS)
- SQL Server: `sql-lunchvote-dev` (Entra ID auth only)
- SQL Database: `sqldb-lunchvote` (Basic tier, 2GB)
- Key Vault: `kv-lunchvote-dev` (RBAC enabled)
- Static Web App: `stapp-lunchvote-dev` (optional, disabled by default)

### Option 2: Deploy with Terraform

#### Deploy Infrastructure

```bash
# Login to Azure
az login

# Get your credentials
$OBJECT_ID = az ad signed-in-user show --query id -o tsv
$EMAIL = az ad signed-in-user show --query userPrincipalName -o tsv

# Navigate to terraform directory
cd infra/terraform

# Initialize Terraform
terraform init

# Review the deployment plan
terraform plan -out tfplan -var="sql_admin_object_id=$OBJECT_ID" -var="sql_admin_login=$EMAIL"

# Deploy infrastructure
terraform apply tfplan

# View outputs
terraform output
```

#### Terraform Resources Created

- Resource Group: `rg-lunchvote-dev`
- Backend App Service Plan: `plan-lunchvote-dev` (Linux, B1 SKU)
- Backend App Service: `app-lunchvote-api-dev-{random}` (.NET 8.0)
- Frontend App Service Plan: `plan-lunchvote-spa-dev` (Linux, B1 SKU)
- Frontend App Service: `app-lunchvote-spa-dev-{random}` (Node.js 20 LTS)
- SQL Server: `sql-lunchvote-dev` (Entra ID auth only)
- SQL Database: `sqldb-lunchvote` (Basic tier, 2GB)
- Key Vault: `kv-lunchvote-dev` (RBAC enabled)
- Static Web App: `stapp-lunchvote-dev` (optional, disabled by default)

**Note:** The Terraform Azure App Service provider currently supports .NET 8.0 as the runtime. While the application code is built with .NET 10.0, it runs on the .NET 8.0 runtime in Azure. For full .NET 10.0 runtime support, use the Bicep deployment option instead.

#### Terraform Configuration

The deployment uses command-line variables. If you want to customize defaults, you can override them:

```bash
# Optional: Override default values
terraform apply \
  -var="sql_admin_object_id=$OBJECT_ID" \
  -var="sql_admin_login=$EMAIL" \
  -var="environment=dev" \
  -var="location=australiaeast" \
  -var="deploy_static_web_app=false"
```

**Note:** Alternatively, you can create an optional `terraform.tfvars` file (already in .gitignore) with these values to avoid passing them on every command.

### Deploy Backend

The deployment process uses zip deployment to Azure App Service.

```bash
cd src/LunchVoteApi

# Build and publish the application
dotnet publish -c Release -o ./publish

# Create a zip archive from the published output
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Deploy the zip archive to Azure App Service
az webapp deploy \
  --resource-group rg-lunchvote-dev \
  --name app-lunchvote-api-dev \
  --src-path ./publish.zip \
  --type zip

# Clean up
Remove-Item ./publish.zip
```

**Note:** The `az webapp deploy` command with `--type zip` performs a zip deployment. 

### Deploy Frontend

```bash
cd src/lunch-vote-spa
npm run build

# Deploy to Frontend App Service
Compress-Archive -Path ./dist/* -DestinationPath ./dist.zip -Force

az webapp deploy \
  --resource-group rg-lunchvote-dev \
  --name app-lunchvote-spa-dev-{random-suffix} \
  --src-path ./dist.zip \
  --type zip

# Clean up
Remove-Item ./dist.zip
```

**Note:** Replace `{random-suffix}` with the actual suffix from your Terraform deployment output.

## Infrastructure Features

Both Bicep and Terraform deployments include:

- ✅ **Passwordless Authentication**: SQL Server uses Microsoft Entra ID authentication
- ✅ **Managed Identity**: App Services use System-Assigned Managed Identity
- ✅ **RBAC Authorization**: Key Vault uses role-based access control
- ✅ **Security**: TLS 1.2+, HTTPS-only, soft delete enabled
- ✅ **Environment Isolation**: Resources named with environment suffix (`-dev`, `-stg`, `-prod`)
- ✅ **CORS Configuration**: Pre-configured for local development
- ✅ **Dual App Service Architecture**: Separate App Services for Backend API and Frontend SPA

## Sample Usage

### Create a Poll

```bash
curl -X POST https://localhost:52544/api/polls \
  -H "Content-Type: application/json" \
  -d '{"groupId":"platform","question":"Where should we eat today?","options":["Sushi","Burgers","Thai","Pizza"]}' \
  -k
```

### Vote

```bash
curl -X POST https://localhost:52544/api/votes \
  -H "Content-Type: application/json" \
  -d '{"pollId":"<poll-id>","optionId":"<option-id>","voterToken":"my-unique-token"}' \
  -k
```

## License

MIT
