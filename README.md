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
- Azure App Service
- Azure SQL Database
- Azure Key Vault
- Azure Static Web Apps (optional)

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker (optional, for local SQL Server)
- Azure CLI
- Bicep CLI or Terraform (for infrastructure deployment)

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

The API will be available at `http://localhost:5000`

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

## Testing

### Backend Tests

```bash
cd tests/LunchVoteApi.Tests
dotnet test
```

### Frontend Tests

```bash
cd src/lunch-vote-spa
npm test
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

### Option 2: Deploy with Terraform

#### Deploy Infrastructure

```bash
# Login to Azure
az login

# Navigate to terraform directory
cd infra/terraform

# Edit variables (update SQL admin credentials)
code terraform.tfvars

# Initialize Terraform
terraform init

# Review the deployment plan
terraform plan

# Deploy infrastructure
terraform apply

# View outputs
terraform output
```

#### Terraform Resources Created

- Resource Group: `rg-lunchvote-dev`
- App Service Plan: `plan-lunchvote-dev` (Linux, B1 SKU)
- App Service: `app-lunchvote-api-dev` (.NET 8.0)
- SQL Server: `sql-lunchvote-dev` (Entra ID auth only)
- SQL Database: `sqldb-lunchvote` (Basic tier, 2GB)
- Key Vault: `kv-lunchvote-dev` (RBAC enabled)
- Static Web App: `stapp-lunchvote-dev` (optional)

#### Terraform Configuration

Edit `infra/terraform/terraform.tfvars`:

```hcl
environment             = "dev"
location                = "australiaeast"
sql_admin_object_id     = "your-object-id"          # Get from: az ad signed-in-user show --query id -o tsv
sql_admin_login         = "your-email@domain.com"   # Get from: az ad signed-in-user show --query userPrincipalName -o tsv
deploy_static_web_app   = false
```

### Deploy Backend

```bash
cd src/LunchVoteApi
dotnet publish -c Release -o ./publish

# Deploy using Azure CLI
az webapp deploy \
  --resource-group rg-lunchvote-dev \
  --name app-lunchvote-api-dev \
  --src-path ./publish \
  --type zip
```

### Deploy Frontend

```bash
cd src/lunch-vote-spa
npm run build

# Deploy to Azure Static Web Apps or copy to API wwwroot
```

## Infrastructure Features

Both Bicep and Terraform deployments include:

- ✅ **Passwordless Authentication**: SQL Server uses Microsoft Entra ID authentication
- ✅ **Managed Identity**: App Service uses System-Assigned Managed Identity
- ✅ **RBAC Authorization**: Key Vault uses role-based access control
- ✅ **Security**: TLS 1.2+, HTTPS-only, soft delete enabled
- ✅ **Environment Isolation**: Resources named with environment suffix (`-dev`, `-stg`, `-prod`)
- ✅ **CORS Configuration**: Pre-configured for local development and Azure Static Apps

## Sample Usage

### Create a Poll

```bash
curl -X POST http://localhost:5000/api/polls \
  -H "Content-Type: application/json" \
  -d '{"groupId":"platform","question":"Where should we eat today?","options":["Sushi","Burgers","Thai","Pizza"]}'
```

### Vote

```bash
curl -X POST http://localhost:5000/api/votes \
  -H "Content-Type: application/json" \
  -d '{"pollId":"<poll-id>","optionId":"<option-id>","voterToken":"my-unique-token"}'
```

## License

MIT
