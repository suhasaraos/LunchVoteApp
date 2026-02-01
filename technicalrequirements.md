# Lunch Vote App - Technical Requirements

## 1. Overview

This document defines the technical requirements for implementing the Lunch Vote App as specified in the business requirements. The solution uses a .NET 10 REST API backend, Azure SQL database, and a React + Vite SPA frontend, all hosted on Azure.

---

## 2. Technology Stack

### 2.1 Backend

| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 10.x |
| Framework | ASP.NET Core Web API | 10.x |
| ORM | Entity Framework Core | 10.x |
| Database | Azure SQL Database | Latest |

### 2.2 Frontend

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | React | 18.x+ |
| Build Tool | Vite | 5.x+ |
| Language | TypeScript | 5.x+ |
| HTTP Client | Fetch API or Axios | Latest |

### 2.3 Infrastructure

| Component | Azure Service |
|-----------|---------------|
| API Hosting | Azure App Service |
| SPA Hosting | Azure Static Web Apps (or served from API) |
| Database | Azure SQL Database |
| Secrets Management | Azure Key Vault |
| Identity | Azure Managed Identity |

---

## 3. System Architecture

### 3.1 High-Level Architecture

```
┌─────────────────┐     HTTPS      ┌─────────────────────┐
│                 │ ◄────────────► │                     │
│   React SPA     │                │   .NET 10 Web API   │
│   (Browser)     │                │   (App Service)     │
│                 │                │                     │
└─────────────────┘                └──────────┬──────────┘
                                              │
                                              │ Managed Identity
                                              ▼
                              ┌───────────────────────────────┐
                              │                               │
                              │       Azure SQL Database      │
                              │                               │
                              └───────────────────────────────┘
                                              ▲
                                              │ Managed Identity
                              ┌───────────────────────────────┐
                              │                               │
                              │       Azure Key Vault         │
                              │                               │
                              └───────────────────────────────┘
```

### 3.2 Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| React SPA | User interface, voter token management, API consumption |
| .NET API | Business logic, data validation, database operations |
| Azure SQL | Data persistence, uniqueness constraints |
| Key Vault | Secure storage of configuration secrets |
| Managed Identity | Passwordless authentication to Azure services |

---

## 4. Database Design

### 4.1 Entity Relationship Diagram

```
┌─────────────────────┐
│        Poll         │
├─────────────────────┤
│ Id (PK, GUID)       │
│ GroupId (nvarchar)  │
│ Question (nvarchar) │
│ IsActive (bit)      │
│ CreatedAt (datetime)│
└─────────┬───────────┘
          │ 1
          │
          │ *
┌─────────▼───────────┐
│       Option        │
├─────────────────────┤
│ Id (PK, GUID)       │
│ PollId (FK, GUID)   │
│ Text (nvarchar)     │
└─────────┬───────────┘
          │ 1
          │
          │ *
┌─────────▼───────────┐
│        Vote         │
├─────────────────────┤
│ Id (PK, GUID)       │
│ PollId (FK, GUID)   │
│ OptionId (FK, GUID) │
│ VoterToken (nvarchar)│
│ CreatedAt (datetime)│
└─────────────────────┘
```

### 4.2 Table Definitions

#### 4.2.1 Poll Table

```sql
CREATE TABLE Poll (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GroupId         NVARCHAR(50)     NOT NULL,
    Question        NVARCHAR(200)    NOT NULL,
    IsActive        BIT              NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Poll_GroupId (GroupId),
    INDEX IX_Poll_GroupId_IsActive (GroupId, IsActive)
);
```

#### 4.2.2 Option Table

```sql
CREATE TABLE [Option] (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PollId          UNIQUEIDENTIFIER NOT NULL,
    Text            NVARCHAR(100)    NOT NULL,
    
    CONSTRAINT FK_Option_Poll FOREIGN KEY (PollId) 
        REFERENCES Poll(Id) ON DELETE CASCADE
);
```

#### 4.2.3 Vote Table

```sql
CREATE TABLE Vote (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PollId          UNIQUEIDENTIFIER NOT NULL,
    OptionId        UNIQUEIDENTIFIER NOT NULL,
    VoterToken      NVARCHAR(64)     NOT NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Vote_Poll FOREIGN KEY (PollId) 
        REFERENCES Poll(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Vote_Option FOREIGN KEY (OptionId) 
        REFERENCES [Option](Id),
    CONSTRAINT UQ_Vote_Poll_VoterToken UNIQUE (PollId, VoterToken)
);
```

### 4.3 Indexes

| Table | Index | Columns | Purpose |
|-------|-------|---------|---------|
| Poll | IX_Poll_GroupId | GroupId | Fast lookup by group |
| Poll | IX_Poll_GroupId_IsActive | GroupId, IsActive | Active poll lookup |
| Vote | UQ_Vote_Poll_VoterToken | PollId, VoterToken | Prevent duplicate votes |

---

## 5. API Specification

### 5.1 Base Configuration

| Setting | Value |
|---------|-------|
| Base URL | `/api` |
| Content-Type | `application/json` |
| Character Encoding | UTF-8 |

### 5.2 Endpoints

#### 5.2.1 Create Poll

**Request**
```
POST /api/polls
Content-Type: application/json
```

**Request Body**
```json
{
  "groupId": "string (required, max 50 chars)",
  "question": "string (required, max 200 chars)",
  "options": ["string (required, max 100 chars each, min 2 options)"]
}
```

**Response - 201 Created**
```json
{
  "pollId": "guid"
}
```

**Response - 400 Bad Request**
```json
{
  "error": "ValidationError",
  "message": "Description of validation failure"
}
```

**Business Logic**
- Set `IsActive = true` for new poll
- Optionally deactivate existing active poll for the same group
- Generate GUIDs for poll and all options

#### 5.2.2 Get Active Poll

**Request**
```
GET /api/polls/active?groupId={groupId}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| groupId | string | Yes | The group identifier |

**Response - 200 OK**
```json
{
  "pollId": "guid",
  "groupId": "string",
  "question": "string",
  "options": [
    {
      "optionId": "guid",
      "text": "string"
    }
  ]
}
```

**Response - 404 Not Found**
```json
{
  "error": "NotFound",
  "message": "No active poll found for this group."
}
```

#### 5.2.3 Submit Vote

**Request**
```
POST /api/votes
Content-Type: application/json
```

**Request Body**
```json
{
  "pollId": "guid (required)",
  "optionId": "guid (required)",
  "voterToken": "string (required, max 64 chars)"
}
```

**Response - 201 Created**
```json
{
  "message": "Vote recorded successfully."
}
```

**Response - 409 Conflict**
```json
{
  "error": "AlreadyVoted",
  "message": "This device has already voted in this poll."
}
```

**Response - 400 Bad Request**
```json
{
  "error": "ValidationError",
  "message": "Invalid poll or option ID."
}
```

**Business Logic**
- Validate that optionId belongs to the specified pollId
- Check unique constraint on (PollId, VoterToken)
- Return 409 if constraint violation

#### 5.2.4 Get Poll Results

**Request**
```
GET /api/polls/{pollId}/results
```

**Path Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| pollId | guid | Yes | The poll identifier |

**Response - 200 OK**
```json
{
  "pollId": "guid",
  "question": "string",
  "results": [
    {
      "optionId": "guid",
      "text": "string",
      "count": 0
    }
  ],
  "totalVotes": 0
}
```

**Response - 404 Not Found**
```json
{
  "error": "NotFound",
  "message": "Poll not found."
}
```

### 5.3 Error Response Format

All error responses follow this structure:

```json
{
  "error": "ErrorCode",
  "message": "Human-readable error description"
}
```

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| ValidationError | 400 | Request validation failed |
| NotFound | 404 | Resource not found |
| AlreadyVoted | 409 | Duplicate vote attempt |
| InternalError | 500 | Unexpected server error |

---

## 6. Backend Implementation Requirements

### 6.1 Project Structure

```
LunchVoteApi/
├── Controllers/
│   ├── PollsController.cs
│   └── VotesController.cs
├── Models/
│   ├── Entities/
│   │   ├── Poll.cs
│   │   ├── Option.cs
│   │   └── Vote.cs
│   ├── DTOs/
│   │   ├── CreatePollRequest.cs
│   │   ├── CreatePollResponse.cs
│   │   ├── ActivePollResponse.cs
│   │   ├── VoteRequest.cs
│   │   └── PollResultsResponse.cs
│   └── ErrorResponse.cs
├── Data/
│   └── LunchVoteDbContext.cs
├── Services/
│   ├── IPollService.cs
│   ├── PollService.cs
│   ├── IVoteService.cs
│   └── VoteService.cs
├── Program.cs
└── appsettings.json
```

### 6.2 Entity Framework Configuration

#### 6.2.1 DbContext

```csharp
public class LunchVoteDbContext : DbContext
{
    public DbSet<Poll> Polls { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<Vote> Votes { get; set; }
}
```

#### 6.2.2 Model Configuration

- Use Fluent API for relationship configuration
- Configure unique index on Vote (PollId, VoterToken)
- Configure cascade delete for Poll → Options
- Configure cascade delete for Poll → Votes

### 6.3 Dependency Injection

Register the following services:

| Service | Lifetime |
|---------|----------|
| DbContext | Scoped |
| IPollService | Scoped |
| IVoteService | Scoped |

### 6.4 Validation Requirements

| Endpoint | Validation Rules |
|----------|------------------|
| POST /api/polls | GroupId required, max 50 chars; Question required, max 200 chars; Options array min 2 items, each max 100 chars |
| POST /api/votes | PollId required, valid GUID; OptionId required, valid GUID; VoterToken required, max 64 chars |

### 6.5 CORS Configuration

Enable CORS for the SPA origin:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins("https://<spa-domain>")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## 7. Frontend Implementation Requirements

### 7.1 Project Structure

```
lunch-vote-spa/
├── src/
│   ├── components/
│   │   ├── VoteScreen.tsx
│   │   ├── ResultsScreen.tsx
│   │   ├── OptionCard.tsx
│   │   └── ResultBar.tsx
│   ├── services/
│   │   ├── api.ts
│   │   └── voterToken.ts
│   ├── types/
│   │   └── index.ts
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

### 7.2 Routing

| Route | Component | Description |
|-------|-----------|-------------|
| `/group/:groupId` | VoteScreen | Displays active poll for group |
| `/poll/:pollId/results` | ResultsScreen | Displays poll results |

### 7.3 Voter Token Management

```typescript
// voterToken.ts
const STORAGE_KEY = 'voterToken';

export function getOrCreateVoterToken(): string {
    let token = localStorage.getItem(STORAGE_KEY);
    if (!token) {
        token = crypto.randomUUID();
        localStorage.setItem(STORAGE_KEY, token);
    }
    return token;
}
```

### 7.4 API Service

```typescript
// api.ts
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

export async function getActivePoll(groupId: string): Promise<ActivePoll>;
export async function submitVote(request: VoteRequest): Promise<void>;
export async function getPollResults(pollId: string): Promise<PollResults>;
```

### 7.5 State Management

- Use React hooks (useState, useEffect) for component state
- Consider React Context for voter token if needed across components
- Handle loading, success, and error states for all API calls

### 7.6 UI/UX Requirements

| Requirement | Implementation |
|-------------|----------------|
| Loading states | Show spinner/skeleton during API calls |
| Error handling | Display user-friendly error messages |
| Vote confirmation | Show "Thank you" message after voting |
| Results display | Show vote counts and percentages |
| Responsive design | Support mobile and desktop viewports |

---

## 8. Azure Infrastructure Requirements

> **Note**: All Azure infrastructure shall be defined using **Bicep IaC templates**. Do not provision resources manually. See Section 8.9 for Bicep module structure.

### 8.1 Resource Group

Create a single resource group containing all resources:

| Resource | Name Pattern |
|----------|--------------|
| Resource Group | `rg-lunchvote-{environment}` |

### 8.2 Azure App Service

| Setting | Value |
|---------|-------|
| Name | `app-lunchvote-api-{environment}` |
| Runtime | .NET 10 |
| OS | Linux (recommended) |
| Plan | B1 or higher for hackathon |
| Managed Identity | System-assigned, Enabled |

### 8.3 Azure SQL Database

| Setting | Value |
|---------|-------|
| Server Name | `sql-lunchvote-{environment}` |
| Database Name | `sqldb-lunchvote` |
| SKU | Basic or S0 for hackathon |
| Authentication | Microsoft Entra ID only |
| Admin | Set Microsoft Entra admin |

### 8.4 Azure Key Vault

| Setting | Value |
|---------|-------|
| Name | `kv-lunchvote-{environment}` |
| SKU | Standard |
| Access | RBAC (recommended) |

### 8.5 Azure Static Web Apps (Optional)

| Setting | Value |
|---------|-------|
| Name | `stapp-lunchvote-{environment}` |
| SKU | Free |
| Build | Vite |

### 8.6 Managed Identity Configuration

#### App Service to SQL Database

1. Enable System Managed Identity on App Service
2. Add App Service identity as user in SQL Database:

```sql
CREATE USER [app-lunchvote-api-dev] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [app-lunchvote-api-dev];
ALTER ROLE db_datawriter ADD MEMBER [app-lunchvote-api-dev];
```

#### App Service to Key Vault

1. Assign "Key Vault Secrets User" role to App Service identity
2. Reference secrets in App Service configuration:
   ```
   @Microsoft.KeyVault(SecretUri=https://kv-lunchvote-dev.vault.azure.net/secrets/SecretName)
   ```

### 8.7 Connection String Configuration

Use Azure SQL with Managed Identity:

```
Server=tcp:sql-lunchvote-dev.database.windows.net,1433;Database=sqldb-lunchvote;Authentication=Active Directory Default;
```

### 8.8 Bicep IaC Structure

All infrastructure shall be defined in Bicep templates under the `infra/` directory:

```
infra/
├── main.bicep              # Main orchestration template
├── main.bicepparam         # Environment parameters
├── modules/
│   ├── appService.bicep    # App Service + Plan
│   ├── sqlDatabase.bicep   # SQL Server + Database
│   ├── keyVault.bicep      # Key Vault
│   └── staticWebApp.bicep  # Static Web App (optional)
└── scripts/
    └── deploy.sh           # Deployment script
```

### 8.9 Bicep Module Requirements

#### 8.9.1 Main Template (main.bicep)

| Parameter | Type | Description |
|-----------|------|-------------|
| environment | string | Environment suffix (dev, stg, prod) |
| location | string | Azure region (default: resourceGroup().location) |
| sqlAdminObjectId | string | Microsoft Entra ID object ID for SQL admin |

#### 8.9.2 App Service Module

- Create App Service Plan (Linux, B1 SKU)
- Create App Service with .NET 10 runtime
- Enable System Managed Identity
- Configure app settings and connection strings
- Output: App Service principal ID, default hostname

#### 8.9.3 SQL Database Module

- Create SQL Server with Microsoft Entra-only authentication
- Create SQL Database (Basic/S0 SKU)
- Configure firewall rules (Allow Azure services)
- Output: SQL Server FQDN, database name

#### 8.9.4 Key Vault Module

- Create Key Vault (Standard SKU)
- Enable RBAC authorization
- Assign "Key Vault Secrets User" role to App Service identity
- Output: Key Vault URI

#### 8.9.5 Role Assignments

- App Service identity → SQL Database (handled via SQL script post-deployment)
- App Service identity → Key Vault Secrets User

### 8.10 Deployment Commands

```bash
# Create resource group
az group create --name rg-lunchvote-dev --location australiaeast

# Deploy infrastructure
az deployment group create \
  --resource-group rg-lunchvote-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam \
  --parameters environment=dev

# Post-deployment: Run SQL script to add managed identity user
# (Bicep cannot create SQL users; requires sqlcmd or Azure CLI)
```

---

## 9. Security Requirements

### 9.1 API Security

| Requirement | Implementation |
|-------------|----------------|
| HTTPS Only | Enforce HTTPS redirect |
| CORS | Restrict to SPA origin |
| Input Validation | Validate all request bodies |
| SQL Injection | Use parameterized queries (EF Core) |

### 9.2 Data Protection

| Requirement | Implementation |
|-------------|----------------|
| No PII Storage | VoterToken is anonymous UUID |
| Encryption at Rest | Azure SQL TDE (default) |
| Encryption in Transit | TLS 1.2+ |

### 9.3 Secret Management

| Secret | Storage Location |
|--------|------------------|
| SQL Connection String | Key Vault (if not using MI) |
| API Keys (if any) | Key Vault |
| App Settings | App Service Configuration |

---

## 10. Development & Deployment

### 10.1 Local Development

| Tool | Purpose |
|------|---------|
| Visual Studio / VS Code | IDE |
| .NET 10 SDK | Backend development |
| Node.js 20+ | Frontend development |
| Docker (optional) | Local SQL Server |
| Azure CLI | Azure resource management |

### 10.2 Local Database Options

1. **SQL Server LocalDB** - Windows only
2. **Docker SQL Server** - Cross-platform
3. **SQLite** - Lightweight alternative for development

### 10.3 Environment Variables

#### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  },
  "AllowedOrigins": ["http://localhost:5173"]
}
```

#### Frontend (.env)

```
VITE_API_URL=http://localhost:5000/api
```

### 10.4 Deployment Pipeline

| Stage | Actions |
|-------|---------|
| Provision Infra | `az deployment group create` with Bicep templates |
| Post-Provision | Run SQL script to create managed identity user |
| Build API | `dotnet build`, `dotnet publish` |
| Build SPA | `npm install`, `npm run build` |
| Deploy API | Azure App Service deployment |
| Deploy SPA | Azure Static Web Apps or copy to API wwwroot |
| Run Migrations | `dotnet ef database update` |

---

## 11. Testing Requirements

### 11.1 Backend Testing

| Test Type | Framework | Coverage |
|-----------|-----------|----------|
| Unit Tests | xUnit | Services, Validation |
| Integration Tests | xUnit + WebApplicationFactory | API Endpoints |

### 11.2 Frontend Testing

| Test Type | Framework | Coverage |
|-----------|-----------|----------|
| Unit Tests | Vitest | Components, Services |
| E2E Tests | Playwright (optional) | User flows |

### 11.3 Key Test Scenarios

| Scenario | Expected Result |
|----------|-----------------|
| Create poll with valid data | 201 Created |
| Create poll with missing fields | 400 Bad Request |
| Get active poll for group | 200 OK with poll data |
| Get active poll for non-existent group | 404 Not Found |
| Submit first vote | 201 Created |
| Submit duplicate vote | 409 Conflict |
| Get results for valid poll | 200 OK with counts |

---

## 12. Monitoring & Logging

### 12.1 Application Insights

- Enable Application Insights on App Service
- Log all API requests and responses
- Track custom events for votes

### 12.2 Logging Levels

| Environment | Level |
|-------------|-------|
| Development | Debug |
| Production | Information |

### 12.3 Key Metrics

| Metric | Purpose |
|--------|---------|
| Request duration | API performance |
| Error rate | Reliability |
| Vote count | Business metric |

---

## 13. Appendix

### 13.1 Environment Naming

| Environment | Suffix |
|-------------|--------|
| Development | `-dev` |
| Staging | `-stg` |
| Production | `-prod` |

### 13.2 Sample Test Data

```json
{
  "groupId": "platform",
  "question": "Where should we eat today?",
  "options": ["Sushi", "Burgers", "Thai", "Pizza"]
}
```

### 13.3 Traceability Matrix

| Business Req | Technical Implementation |
|--------------|--------------------------|
| GRP-01 | GroupId field in Poll table |
| GRP-04 | `/group/:groupId` route |
| POL-04 | IsActive flag with query filter |
| VOT-03 | UNIQUE constraint on (PollId, VoterToken) |
| DUP-01 | `crypto.randomUUID()` in frontend |
| DUP-02 | localStorage persistence |
| RES-02 | COUNT aggregation in results query |
