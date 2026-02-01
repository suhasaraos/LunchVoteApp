# Lunch Vote App - Implementation Plan

## Overview

This document outlines the detailed implementation plan for the Lunch Vote App based on the business and technical requirements. The application enables teams to vote on lunch options using a .NET 10 API backend, React + Vite frontend, and Azure infrastructure provisioned via Bicep IaC.

---

## Phase 1: Project Setup & Infrastructure

### 1.1 Create Solution Structure

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 1.1.1 | Create root solution folder `LunchVoteApp/` |
| 1.1.2 | Create .NET 10 Web API project `src/LunchVoteApi/` |
| 1.1.3 | Create React + Vite project `src/lunch-vote-spa/` |
| 1.1.4 | Create infrastructure folder `infra/` |
| 1.1.5 | Create solution file `LunchVoteApp.sln` |
| 1.1.6 | Add `.gitignore` for .NET, Node.js, and IDE files |
| 1.1.7 | Add `README.md` with project overview |

**Expected Structure**:
```
LunchVoteApp/
├── src/
│   ├── LunchVoteApi/
│   └── lunch-vote-spa/
├── infra/
├── LunchVoteApp.sln
├── .gitignore
└── README.md
```

---

### 1.2 Create Bicep Infrastructure Templates

**Duration**: 1 hour

| Task | Details |
|------|---------|
| 1.2.1 | Create `infra/main.bicep` - Main orchestration template |
| 1.2.2 | Create `infra/main.bicepparam` - Parameter file for dev environment |
| 1.2.3 | Create `infra/modules/appService.bicep` - App Service + Plan |
| 1.2.4 | Create `infra/modules/sqlDatabase.bicep` - SQL Server + Database |
| 1.2.5 | Create `infra/modules/keyVault.bicep` - Key Vault with RBAC |
| 1.2.6 | Create `infra/modules/staticWebApp.bicep` - Static Web App (optional) |
| 1.2.7 | Create `infra/scripts/deploy.sh` - Deployment automation script |
| 1.2.8 | Create `infra/scripts/create-sql-user.sql` - SQL user creation script |

**Module Specifications**:

#### main.bicep
- Parameters: `environment`, `location`, `sqlAdminObjectId`, `sqlAdminLogin`
- Deploy all modules with consistent naming
- Output all resource endpoints

#### appService.bicep
- App Service Plan: Linux, B1 SKU
- App Service: .NET 10 runtime
- System Managed Identity enabled
- App Settings: `ASPNETCORE_ENVIRONMENT`, connection strings
- HTTPS only enforced

#### sqlDatabase.bicep
- SQL Server with Microsoft Entra-only authentication
- SQL Database: Basic SKU (5 DTU)
- Firewall: Allow Azure services
- TDE enabled (default)

#### keyVault.bicep
- Standard SKU
- RBAC authorization mode
- Role assignment: App Service identity → Key Vault Secrets User

---

## Phase 2: Backend API Development

### 2.1 Project Configuration

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 2.1.1 | Configure `LunchVoteApi.csproj` with required packages |
| 2.1.2 | Configure `Program.cs` with services and middleware |
| 2.1.3 | Configure `appsettings.json` and `appsettings.Development.json` |
| 2.1.4 | Add CORS policy for SPA origin |
| 2.1.5 | Add global exception handler middleware |

**Required NuGet Packages**:
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Azure.Identity` (for Managed Identity)
- `Microsoft.ApplicationInsights.AspNetCore` (optional)

---

### 2.2 Entity Models

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 2.2.1 | Create `Models/Entities/Poll.cs` |
| 2.2.2 | Create `Models/Entities/Option.cs` |
| 2.2.3 | Create `Models/Entities/Vote.cs` |

**Poll.cs**:
```csharp
public class Poll
{
    public Guid Id { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Option> Options { get; set; } = new();
    public List<Vote> Votes { get; set; } = new();
}
```

**Option.cs**:
```csharp
public class Option
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string Text { get; set; } = string.Empty;
    public Poll Poll { get; set; } = null!;
    public List<Vote> Votes { get; set; } = new();
}
```

**Vote.cs**:
```csharp
public class Vote
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
    public string VoterToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Poll Poll { get; set; } = null!;
    public Option Option { get; set; } = null!;
}
```

---

### 2.3 DTOs

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 2.3.1 | Create `Models/DTOs/CreatePollRequest.cs` |
| 2.3.2 | Create `Models/DTOs/CreatePollResponse.cs` |
| 2.3.3 | Create `Models/DTOs/ActivePollResponse.cs` |
| 2.3.4 | Create `Models/DTOs/VoteRequest.cs` |
| 2.3.5 | Create `Models/DTOs/PollResultsResponse.cs` |
| 2.3.6 | Create `Models/ErrorResponse.cs` |

**Validation Attributes**:
- `CreatePollRequest`: GroupId (Required, MaxLength 50), Question (Required, MaxLength 200), Options (MinLength 2, each MaxLength 100)
- `VoteRequest`: PollId (Required), OptionId (Required), VoterToken (Required, MaxLength 64)

---

### 2.4 Database Context

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 2.4.1 | Create `Data/LunchVoteDbContext.cs` |
| 2.4.2 | Configure entity relationships with Fluent API |
| 2.4.3 | Configure indexes |
| 2.4.4 | Create initial migration |

**Key Configurations**:
- Poll → Options: One-to-Many, Cascade Delete
- Poll → Votes: One-to-Many, Cascade Delete
- Option → Votes: One-to-Many, Restrict Delete
- Unique Index: Vote (PollId, VoterToken)
- Index: Poll (GroupId, IsActive)

---

### 2.5 Services Layer

**Duration**: 1 hour

| Task | Details |
|------|---------|
| 2.5.1 | Create `Services/IPollService.cs` interface |
| 2.5.2 | Create `Services/PollService.cs` implementation |
| 2.5.3 | Create `Services/IVoteService.cs` interface |
| 2.5.4 | Create `Services/VoteService.cs` implementation |

**IPollService Methods**:
```csharp
Task<Guid> CreatePollAsync(CreatePollRequest request);
Task<ActivePollResponse?> GetActivePollAsync(string groupId);
Task<PollResultsResponse?> GetPollResultsAsync(Guid pollId);
```

**IVoteService Methods**:
```csharp
Task<VoteResult> SubmitVoteAsync(VoteRequest request);
```

**Business Logic**:
- `CreatePollAsync`: Deactivate existing active poll for group, create new poll with options
- `GetActivePollAsync`: Find active poll by GroupId, include options
- `GetPollResultsAsync`: Aggregate vote counts per option
- `SubmitVoteAsync`: Validate option belongs to poll, handle duplicate vote exception

---

### 2.6 Controllers

**Duration**: 45 minutes

| Task | Details |
|------|---------|
| 2.6.1 | Create `Controllers/PollsController.cs` |
| 2.6.2 | Create `Controllers/VotesController.cs` |

**PollsController Endpoints**:
| Method | Route | Action |
|--------|-------|--------|
| POST | `/api/polls` | CreatePoll |
| GET | `/api/polls/active` | GetActivePoll (query: groupId) |
| GET | `/api/polls/{pollId}/results` | GetPollResults |

**VotesController Endpoints**:
| Method | Route | Action |
|--------|-------|--------|
| POST | `/api/votes` | SubmitVote |

---

### 2.7 Error Handling

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 2.7.1 | Create custom exception classes (e.g., `DuplicateVoteException`) |
| 2.7.2 | Create global exception handler middleware |
| 2.7.3 | Map exceptions to appropriate HTTP status codes |

**Exception Mapping**:
| Exception | HTTP Status | Error Code |
|-----------|-------------|------------|
| ValidationException | 400 | ValidationError |
| NotFoundException | 404 | NotFound |
| DuplicateVoteException | 409 | AlreadyVoted |
| DbUpdateException (unique constraint) | 409 | AlreadyVoted |
| Exception | 500 | InternalError |

---

## Phase 3: Frontend Development

### 3.1 Project Setup

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 3.1.1 | Initialize Vite + React + TypeScript project |
| 3.1.2 | Install dependencies: `react-router-dom` |
| 3.1.3 | Configure `vite.config.ts` with API proxy for development |
| 3.1.4 | Create `.env` and `.env.example` files |
| 3.1.5 | Configure `tsconfig.json` with path aliases |

**Commands**:
```bash
npm create vite@latest lunch-vote-spa -- --template react-ts
cd lunch-vote-spa
npm install react-router-dom
```

---

### 3.2 Type Definitions

**Duration**: 15 minutes

| Task | Details |
|------|---------|
| 3.2.1 | Create `src/types/index.ts` with all API types |

**Types**:
```typescript
interface ActivePoll {
  pollId: string;
  groupId: string;
  question: string;
  options: PollOption[];
}

interface PollOption {
  optionId: string;
  text: string;
}

interface VoteRequest {
  pollId: string;
  optionId: string;
  voterToken: string;
}

interface PollResults {
  pollId: string;
  question: string;
  results: OptionResult[];
  totalVotes: number;
}

interface OptionResult {
  optionId: string;
  text: string;
  count: number;
}

interface ApiError {
  error: string;
  message: string;
}
```

---

### 3.3 Services

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 3.3.1 | Create `src/services/voterToken.ts` |
| 3.3.2 | Create `src/services/api.ts` |

**voterToken.ts**:
- `getOrCreateVoterToken()`: Get from localStorage or generate with `crypto.randomUUID()`

**api.ts**:
- `getActivePoll(groupId: string): Promise<ActivePoll>`
- `submitVote(request: VoteRequest): Promise<void>`
- `getPollResults(pollId: string): Promise<PollResults>`
- Handle errors and throw typed `ApiError`

---

### 3.4 Components

**Duration**: 2 hours

| Task | Details |
|------|---------|
| 3.4.1 | Create `src/components/OptionCard.tsx` - Single voting option |
| 3.4.2 | Create `src/components/ResultBar.tsx` - Result bar with percentage |
| 3.4.3 | Create `src/components/VoteScreen.tsx` - Main voting screen |
| 3.4.4 | Create `src/components/ResultsScreen.tsx` - Results display |
| 3.4.5 | Create `src/components/LoadingSpinner.tsx` - Loading indicator |
| 3.4.6 | Create `src/components/ErrorMessage.tsx` - Error display |

**VoteScreen.tsx**:
- Fetch active poll on mount using `groupId` from URL params
- Display question and options
- Handle vote submission
- Show success message and link to results
- Handle "already voted" error gracefully

**ResultsScreen.tsx**:
- Fetch poll results on mount using `pollId` from URL params
- Display question, options with counts and percentages
- Visual bar chart representation
- Auto-refresh every 10 seconds (optional for near real-time)

---

### 3.5 Routing & App Structure

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 3.5.1 | Configure `src/App.tsx` with React Router |
| 3.5.2 | Update `src/main.tsx` entry point |
| 3.5.3 | Add basic CSS/styling (or integrate Tailwind CSS) |

**Routes**:
| Path | Component |
|------|-----------|
| `/` | Home/Landing (optional) |
| `/group/:groupId` | VoteScreen |
| `/poll/:pollId/results` | ResultsScreen |

---

### 3.6 Styling

**Duration**: 1 hour

| Task | Details |
|------|---------|
| 3.6.1 | Add responsive CSS for mobile and desktop |
| 3.6.2 | Style voting cards with hover/selection states |
| 3.6.3 | Style results bars with animations |
| 3.6.4 | Add loading spinner styles |
| 3.6.5 | Add error message styles |

---

## Phase 4: Integration & Testing

### 4.1 Local Development Setup

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 4.1.1 | Set up local SQL Server (Docker or LocalDB) |
| 4.1.2 | Run EF migrations to create database |
| 4.1.3 | Configure frontend proxy to backend |
| 4.1.4 | Test full flow locally |

**Docker SQL Server**:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-lunchvote \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**Local Connection String**:
```
Server=localhost,1433;Database=LunchVote;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
```

---

### 4.2 Backend Unit Tests

**Duration**: 1 hour

| Task | Details |
|------|---------|
| 4.2.1 | Create test project `tests/LunchVoteApi.Tests/` |
| 4.2.2 | Add xUnit, Moq, FluentAssertions packages |
| 4.2.3 | Write tests for `PollService` |
| 4.2.4 | Write tests for `VoteService` |
| 4.2.5 | Write tests for validation logic |

**Key Test Cases**:
- Create poll with valid data → Success
- Create poll with < 2 options → Validation error
- Get active poll for existing group → Returns poll
- Get active poll for non-existent group → Returns null
- Submit vote → Success
- Submit duplicate vote → Throws exception

---

### 4.3 Backend Integration Tests

**Duration**: 1 hour

| Task | Details |
|------|---------|
| 4.3.1 | Create integration test class using `WebApplicationFactory` |
| 4.3.2 | Use in-memory database for tests |
| 4.3.3 | Test all API endpoints end-to-end |

**Key Test Cases**:
- POST /api/polls → 201 Created
- GET /api/polls/active?groupId=test → 200 OK
- GET /api/polls/active?groupId=unknown → 404 Not Found
- POST /api/votes → 201 Created
- POST /api/votes (duplicate) → 409 Conflict
- GET /api/polls/{id}/results → 200 OK with counts

---

### 4.4 Frontend Tests

**Duration**: 1 hour

| Task | Details |
|------|---------|
| 4.4.1 | Configure Vitest for testing |
| 4.4.2 | Write tests for `voterToken` service |
| 4.4.3 | Write tests for `api` service (with mocked fetch) |
| 4.4.4 | Write component tests for VoteScreen |
| 4.4.5 | Write component tests for ResultsScreen |

---

## Phase 5: Deployment

### 5.1 Provision Azure Infrastructure

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 5.1.1 | Log in to Azure CLI: `az login` |
| 5.1.2 | Create resource group |
| 5.1.3 | Deploy Bicep templates |
| 5.1.4 | Run SQL user creation script |
| 5.1.5 | Verify all resources created |

**Commands**:
```bash
# Create resource group
az group create --name rg-lunchvote-dev --location australiaeast

# Deploy infrastructure
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam \
  --parameters environment=dev

# Get outputs
az deployment group show \
  --resource-group rg-lunchvote-dev \
  --name main \
  --query properties.outputs
```

---

### 5.2 Deploy Backend

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 5.2.1 | Build and publish API |
| 5.2.2 | Run EF migrations against Azure SQL |
| 5.2.3 | Deploy to Azure App Service |
| 5.2.4 | Verify API endpoints |

**Commands**:
```bash
# Build and publish
cd src/LunchVoteApi
dotnet publish -c Release -o ./publish

# Run migrations
dotnet ef database update --connection "<azure-connection-string>"

# Deploy to App Service
az webapp deploy \
  --resource-group rg-lunchvote-dev \
  --name app-lunchvote-api-dev \
  --src-path ./publish \
  --type zip
```

---

### 5.3 Deploy Frontend

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 5.3.1 | Build frontend with production API URL |
| 5.3.2 | Deploy to Azure Static Web Apps (or copy to API wwwroot) |
| 5.3.3 | Verify SPA routing works |

**Commands**:
```bash
# Build frontend
cd src/lunch-vote-spa
npm run build

# Deploy to Static Web Apps
az staticwebapp deploy \
  --name stapp-lunchvote-dev \
  --source ./dist
```

---

### 5.4 End-to-End Verification

**Duration**: 30 minutes

| Task | Details |
|------|---------|
| 5.4.1 | Create a test poll via API |
| 5.4.2 | Access vote screen via browser |
| 5.4.3 | Submit a vote |
| 5.4.4 | Verify results display |
| 5.4.5 | Attempt duplicate vote (should be rejected) |
| 5.4.6 | Test from different browser/device |

---

## Summary

### Estimated Timeline

| Phase | Duration |
|-------|----------|
| Phase 1: Setup & Infrastructure | 1.5 hours |
| Phase 2: Backend Development | 4 hours |
| Phase 3: Frontend Development | 4.5 hours |
| Phase 4: Testing | 3.5 hours |
| Phase 5: Deployment | 2 hours |
| **Total** | **~15.5 hours** |

### Deliverables

| Deliverable | Location |
|-------------|----------|
| .NET 10 API | `src/LunchVoteApi/` |
| React SPA | `src/lunch-vote-spa/` |
| Bicep IaC | `infra/` |
| Unit Tests | `tests/LunchVoteApi.Tests/` |
| Documentation | `README.md` |

### Key Dependencies

| Dependency | Version |
|------------|---------|
| .NET SDK | 10.x |
| Node.js | 20.x+ |
| Azure CLI | Latest |
| Docker (optional) | Latest |

---

## Appendix: Quick Start Commands

### Local Development
```bash
# Start SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Start API
cd src/LunchVoteApi
dotnet run

# Start Frontend
cd src/lunch-vote-spa
npm run dev
```

### Create Sample Poll
```bash
curl -X POST http://localhost:5000/api/polls \
  -H "Content-Type: application/json" \
  -d '{"groupId":"platform","question":"Where should we eat today?","options":["Sushi","Burgers","Thai","Pizza"]}'
```

### Test Voting
```bash
# Get active poll
curl "http://localhost:5000/api/polls/active?groupId=platform"

# Submit vote
curl -X POST http://localhost:5000/api/votes \
  -H "Content-Type: application/json" \
  -d '{"pollId":"<poll-id>","optionId":"<option-id>","voterToken":"test-token-123"}'

# Get results
curl "http://localhost:5000/api/polls/<poll-id>/results"
```
