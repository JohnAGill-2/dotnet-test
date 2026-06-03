# Sportsbook Recommendation Engine

A full-stack application for recommending betting options to players based on their preferences, risk level, and compliance restrictions. Features Claude AI integration for personalized messaging and a Vue.js admin dashboard.

## Project Structure

```
dotnet-test/
├── backend/                    # .NET REST API backend
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Migrations/
│   ├── Properties/
│   ├── dotnet-test.csproj
│   ├── dotnet-test.sln
│   ├── dotnet-test.Tests/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── dotnet-test.http        # API request examples
│   └── bin/, obj/              # Build artifacts
├── frontend/                   # Vue.js admin UI (coming in Week 4)
│   ├── src/
│   ├── package.json
│   ├── vite.config.js
│   └── ...
└── README.md (this file)
```

## Getting Started

### Quick Start (Monorepo)

From the workspace root, install dependencies and run both backend and frontend simultaneously:

```bash
# Install dependencies for both backend and frontend
npm install:all

# Start both services in parallel
npm run dev
```

This will start:
- **Backend API**: http://localhost:5041 (Swagger docs at `/swagger/index.html`)
- **Frontend UI**: http://localhost:5173

### Individual Setup (if needed)

**Backend only**:
```bash
cd backend
dotnet build dotnet-test.sln
dotnet test dotnet-test.sln
dotnet run --project dotnet-test.csproj
```

**Frontend only**:
```bash
cd frontend
npm install
npm run dev
```

**Demo Guide**: See [DEMO.md](DEMO.md) for a complete walk-through of the admin UI and recommendation flow.

## Key Features

### Week 3: Claude Integration ✅
- RESTful API for player data and recommendations
- Deterministic recommendation engine with guardrails (self-exclusion, risk-based blocking)
- Claude AI integration for personalized message generation
- Graceful fallback to deterministic content when Claude is unavailable
- Structured JSON logging via Serilog (coming in Week 4)
- 8 passing unit tests

### Week 4: Admin UI & Demo ✅
- Vue.js admin dashboard with player list, detail, and recommendation viewer
- Tailwind CSS styling with professional UI
- Structured logging with Serilog (JSON to console)
- Global error handling middleware with consistent error responses
- Environment variable configuration (API base URL)
- Demo script and walkthrough guide
- 8 passing unit tests

## API Endpoints

All endpoints use `PlayerId` (business identifier) rather than internal database ID.

- `GET /api/players` - List all players
- `GET /api/players/{playerId}` - Get player details
- `POST /api/players` - Create a player
- `PUT /api/players/{playerId}` - Update a player
- `DELETE /api/players/{playerId}` - Delete a player
- `POST /api/players/{playerId}/recommendations` - Get recommendations for a player

See `backend/dotnet-test.http` for example requests.

## Configuration

### Backend (dotnet)

Configuration is managed via:
- `backend/appsettings.json` - Default settings
- `backend/appsettings.Development.json` - Development overrides (connection strings)
- Environment variables - Secrets (Claude API key via `Claude__ApiKey`)

### Frontend (Vue)

Configuration is managed via:
- `.env.example` - Template for required environment variables
- `.env.local` - Local development overrides (not tracked in git)

## Seeded Data

The database is seeded with 13 test players with varied profiles to test the recommendation rule set:

| PlayerId | Sport | Team | Risk Level | Special Notes |
|----------|-------|------|-----------|---|
| 12345 | Football | Scotland | Low | Consistent bettor, good affinity scores |
| 12346 | Tennis | Andy Murray | Medium | New player, high bet type affinity |
| 12347 | Horse Racing | N/A | High | High-risk player (blocks Bet Builder, Accumulators) |
| 12348 | Cricket | England | Low | 🚫 **Self-excluded** (hard block) |
| 12349 | Rugby | Wales | Medium | ⏸️ **Active cooling-off period** (7 days remaining) |
| 12350 | Basketball | Chicago Bulls | Low | 🔒 **Jurisdiction-restricted** (compliance block) |
| 12351 | Football | Manchester United | Low | Brand new player (3 days) – low affinity |
| 12352 | Football | Liverpool | Medium | Very active (logs in daily) – high affinity |
| 12353 | Tennis | Novak Djokovic | High | Dormant (65 days inactive) – inactive penalty |
| 12354 | Horse Racing | N/A | Very High | Extreme high-stakes player (£250+ avg, £500+ variance) |
| 12355 | Golf | Rory McIlroy | Low | Conservative bettor (£2.50 avg) – highly consistent |
| 12356 | Darts | Luke Humphries | Medium | Mixed preferences – low affinity scores |
| 12357 | Ice Hockey | Vegas Golden Knights | Medium | **Ideal player** (high affinity, consistent, medium risk) |

**Hard Blocks** (return `blocked: true`):
- Players 12348 (self-excluded), 12349 (cooling-off), 12350 (restricted)

**Soft Guardrails** (option filtering):
- Players 12347 and 12354 (high/very high risk) have Bet Builder and Accumulators blocked

**Scoring Penalties**:
- Player 12353 (inactive 65+ days)
- Player 12354 (extreme stakes, high variance)  
- Player 12356 (inconsistent, low affinity)

## Testing

Run the test suite from the backend folder:

```bash
cd backend
dotnet test dotnet-test.sln
```

Current test coverage: 8 passing tests covering recommendation guardrails, content generation, and fallback behavior.

## Environment

- **.NET**: 9.0
- **Database**: PostgreSQL (Docker container)
- **Frontend**: Vue 3, Vite, Tailwind CSS (coming in Week 4)
- **Logging**: Serilog with structured JSON output (coming in Week 4)

## Demo Scenario

The demo walks through:
1. List all players
2. Select player 12345 (Scotland, Low risk)
3. View player profile
4. Trigger recommendations
5. Display recommendation content (Claude-generated or deterministic fallback)

Target time: < 5 minutes


The MVP is now complete with all core features implemented:
- ✅ REST API with player management and recommendations
- ✅ Claude AI integration for personalized messaging
- ✅ Deterministic guardrails and fallback logic
- ✅ Admin UI for testing and demonstration
- ✅ Structured logging throughout the stack
- ✅ Comprehensive error handling

Future enhancements could include:
- Production build optimization (fix Tailwind CSS build issue)
- CORS configuration for multi-origin deployments
- Advanced error boundaries in the UI
- Logging aggregation and dashboards
- End-to-end testing (Cypress/Playwright)
- Rate limiting and authentication
- Database connection pooling optimiz
- [ ] Demo rehearsal and documentation
