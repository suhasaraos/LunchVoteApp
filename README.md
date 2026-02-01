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
├── infra/                     # Azure Bicep IaC templates
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

### Deploy Infrastructure

```bash
# Create resource group
az group create --name rg-lunchvote-dev --location australiaeast

# Deploy Bicep templates
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

### Deploy Backend

```bash
cd src/LunchVoteApi
dotnet publish -c Release -o ./publish
az webapp deploy --resource-group rg-lunchvote-dev --name app-lunchvote-api-dev --src-path ./publish --type zip
```

### Deploy Frontend

```bash
cd src/lunch-vote-spa
npm run build
# Deploy to Azure Static Web Apps or copy to API wwwroot
```

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
