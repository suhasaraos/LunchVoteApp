# Lunch Vote App - Business Requirements

## 1. Overview

A team-based lunch voting application that enables groups within an organization to democratically decide on lunch options. Each team can run independent polls, and participants can vote once per poll from their device.

---

## 2. Business Objectives

- Enable teams to quickly gather lunch preferences
- Provide a simple, frictionless voting experience without requiring user authentication
- Support multiple independent teams/groups voting simultaneously
- Display real-time voting results for transparency
- Demonstrate cloud-native architecture patterns for a hackathon setting

---

## 3. User Roles

### 3.1 Participant
- Views the active poll for their team
- Casts a single vote per poll
- Views poll results after voting

### 3.2 Organizer (Optional)
- Creates a new poll for a specific team/group
- Defines the poll question and available options

---

## 4. Functional Requirements

### 4.1 Group/Team Management

| ID | Requirement |
|----|-------------|
| GRP-01 | The system shall support multiple independent groups (e.g., "platform", "security", "apps") |
| GRP-02 | Each poll shall belong to exactly one group |
| GRP-03 | Groups shall be identified by a unique GroupId |
| GRP-04 | Users shall be able to access their group's poll via URL (e.g., `/group/platform`) |

### 4.2 Poll Creation

| ID | Requirement |
|----|-------------|
| POL-01 | An organizer shall be able to create a new poll for a specific group |
| POL-02 | Each poll shall have a question (up to 200 characters) |
| POL-03 | Each poll shall have multiple options (up to 100 characters each) |
| POL-04 | Only one poll shall be active per group at any time |
| POL-05 | Creating a new poll should automatically deactivate any existing active poll for that group (optional behavior) |
| POL-06 | Each poll shall record its creation timestamp |

### 4.3 Voting

| ID | Requirement |
|----|-------------|
| VOT-01 | A participant shall be able to cast one vote per poll |
| VOT-02 | A participant shall select exactly one option when voting |
| VOT-03 | The system shall prevent duplicate votes from the same device/browser |
| VOT-04 | If a duplicate vote is attempted, the system shall reject it with a clear message |
| VOT-05 | After successfully voting, the participant shall see a confirmation message |
| VOT-06 | After voting, the participant shall be directed to view results |
| VOT-07 | Each vote shall record its submission timestamp |

### 4.4 Vote Deduplication (No-Auth Approach)

| ID | Requirement |
|----|-------------|
| DUP-01 | Each browser/device shall generate a unique voter token on first visit |
| DUP-02 | The voter token shall persist in the browser's local storage |
| DUP-03 | The system shall enforce one vote per poll per voter token |
| DUP-04 | Duplicate vote attempts shall return an "Already Voted" error |

> **Note**: Users can bypass this by clearing browser storage. This is acceptable for a hackathon/sandbox environment.

### 4.5 Results Display

| ID | Requirement |
|----|-------------|
| RES-01 | Results shall be viewable for any poll |
| RES-02 | Results shall display each option with its vote count |
| RES-03 | Results shall display the percentage of votes for each option |
| RES-04 | Results shall display the total number of votes cast |
| RES-05 | Results should update in real-time or near real-time |

---

## 5. User Interface Requirements

### 5.1 Vote Screen

| ID | Requirement |
|----|-------------|
| UI-VOT-01 | Display the poll question prominently |
| UI-VOT-02 | Display all voting options clearly |
| UI-VOT-03 | Provide a vote/submit button |
| UI-VOT-04 | Allow group selection or read group from URL |
| UI-VOT-05 | Show confirmation message after successful vote |
| UI-VOT-06 | Provide link to results after voting |

### 5.2 Results Screen

| ID | Requirement |
|----|-------------|
| UI-RES-01 | Display the poll question |
| UI-RES-02 | Show each option with vote count |
| UI-RES-03 | Show percentage for each option |
| UI-RES-04 | Display total votes cast |
| UI-RES-05 | Visual representation of results (e.g., bar chart) is desirable |

### 5.3 Navigation

| ID | Requirement |
|----|-------------|
| UI-NAV-01 | URL pattern `/group/:groupId` shall show the vote screen for that group |
| UI-NAV-02 | URL pattern `/poll/:pollId/results` shall show results for that poll |

---

## 6. Data Requirements

### 6.1 Poll Entity

| Field | Description | Constraints |
|-------|-------------|-------------|
| Id | Unique identifier | Required, GUID |
| GroupId | Team/group identifier | Required, max 50 characters |
| Question | Poll question text | Required, max 200 characters |
| IsActive | Whether poll is currently active | Required, boolean |
| CreatedAt | Creation timestamp | Required, datetime |

### 6.2 Option Entity

| Field | Description | Constraints |
|-------|-------------|-------------|
| Id | Unique identifier | Required, GUID |
| PollId | Reference to parent poll | Required, foreign key |
| Text | Option text | Required, max 100 characters |

### 6.3 Vote Entity

| Field | Description | Constraints |
|-------|-------------|-------------|
| Id | Unique identifier | Required, GUID |
| PollId | Reference to poll | Required, foreign key |
| OptionId | Reference to selected option | Required, foreign key |
| VoterToken | Device/browser identifier | Required, max 64 characters |
| CreatedAt | Vote timestamp | Required, datetime |

### 6.4 Data Integrity Rules

| ID | Rule |
|----|------|
| DAT-01 | A vote's OptionId must belong to the referenced PollId |
| DAT-02 | Combination of PollId + VoterToken must be unique (prevents duplicate votes) |
| DAT-03 | Options must belong to a valid Poll |

---

## 7. Non-Functional Requirements

### 7.1 Availability
- The application shall be hosted on Azure cloud infrastructure
- Target availability: Standard Azure App Service SLA

### 7.2 Security
- No user authentication required (acceptable for hackathon scope)
- API shall use Managed Identity for database access (no credentials in code)
- Secrets shall be stored in Azure Key Vault

### 7.3 Performance
- Vote submission should complete within 2 seconds
- Results page should load within 3 seconds

### 7.4 Scalability
- Support multiple concurrent groups voting simultaneously
- Handle typical team sizes (10-50 participants per group)

---

## 8. Assumptions & Constraints

### Assumptions
1. Users have access to a modern web browser
2. Users have a stable internet connection
3. Team/group identifiers are pre-defined and known to participants
4. One active poll per group is sufficient

### Constraints
1. No user authentication system (by design for simplicity)
2. Vote deduplication relies on browser local storage (can be bypassed)
3. This is a hackathon/sandbox application, not production-grade

---

## 9. Out of Scope

- User registration and authentication
- Poll scheduling (start/end times)
- Poll templates or recurring polls
- Mobile native applications
- Email/push notifications
- Poll history and analytics dashboard
- Admin panel for user management
- Multi-language support
