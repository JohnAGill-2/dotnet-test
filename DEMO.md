# Week 4 Demo Guide

## Setup

From the workspace root, run the following commands:

```bash
# Install dependencies for both backend and frontend
npm install:all

# Start both services in parallel
npm run dev
```

This will start both services simultaneously:
- **Backend API**: http://localhost:5041 (Swagger docs at `/swagger/index.html`)
- **Frontend UI**: http://localhost:5173

You'll see output from both services in your terminal, prefixed with `[backend]` and `[frontend]`.

## Demo Scenario (< 5 minutes)

### Step 1: Open the Admin UI
- Open http://localhost:5173 in your browser
- You'll see a list of 3 seeded players

### Step 2: Select Player 12345 (Scotland, Low Risk)
- Click on the card for player "12345" (Football, Scotland, Low risk)
- This navigates to `/players/12345` showing player details

### Step 3: Review Player Profile
- Observe the player's betting metrics:
  - Sport: Football
  - Team: Scotland
  - Risk Level: Low
  - Average Stake: £12.50
  - Days Since Joined: 180
- Note: This player has **no restrictions** (not self-excluded, no cooling-off, not jurisdiction-restricted)

### Step 4: Trigger Recommendations
- Click the blue "Get Recommendations" button
- The UI calls `POST /api/players/12345/recommendations`
- The backend:
  1. Evaluates the player through the deterministic recommendation engine
  2. Generates up to 3 allowed options with scores
  3. Calls Claude (if API key is configured) to generate personalized messaging
  4. Falls back to deterministic content if Claude is unavailable
  5. Returns the full recommendation response

### Step 5: View Recommendation Output
- The UI navigates to the recommendation output page
- You'll see:
  - **Headline**: Claude-generated or deterministic (e.g., "Scotland markets are available")
  - **Message**: Personalized content based on the player's preferences
  - **Recommendation Type**: "Content" or "SaferGambling"
  - **Allowed Options**: List of 3 recommendation options, each with a score (0-1)
  - **Audit Trail**: The deterministic rules that were applied (sport affinity, risk checks, etc.)

### Step 6: (Optional) Show Fallback Behavior
- Go back and select player 12347 (Horse Racing, **High Risk**)
- Click "Get Recommendations"
- This player will see:
  - **Blocked Options**: Bet Builder and Accumulator are blocked (high-risk guardrail)
  - **Fallback Content**: "Keep your play in control" (deterministic fallback because of risk profile)
- This demonstrates the guardrail system at work

## Architecture (What's Happening Under the Hood)

### Request Flow

```
Browser                    Vite Dev Server            .NET Backend           Claude API
  |                               |                        |                     |
  | GET /                         |                        |                     |
  |------- load Vue app --------> |                        |                     |
  |                        <------ HTML/JS ----            |                     |
  |                                                        |                     |
  | GET /api/players              |                        |                     |
  |                    --------> (axios call)---> |                        |                     |
  |                                                 query players from DB        |
  |                        <------- JSON --------- |                     |
  |                                                        |                     |
  | POST /api/players/{playerId}/recommendations          |                     |
  |                    --------> (axios call)---> |                        |                     |
  |                                                 ->run engine                 |
  |                                                 ->generate content---------> |
  |                                                 (with fallback)   <----JSON---|
  |                        <------- JSON --------- |                     |
```

### Logging

**Backend** emits structured JSON logs to console:
```
[2026-06-01 12:42:00.123 +00:00] [INF] Request started: GET /api/players/12345
[2026-06-01 12:42:00.150 +00:00] [INF] Claude API key is not configured. Using deterministic recommendation content.
[2026-06-01 12:42:00.160 +00:00] [INF] Request finished: 200 OK
```

**Frontend** logs API calls to the browser console (F12 Developer Tools).

## Configuration

### Backend
- API base URL: `http://localhost:5041`
- Claude API key: Set via environment variable `Claude__ApiKey`
  - If not set, deterministic fallback is used

### Frontend  
- API base URL: Set in `.env.local` to `http://localhost:5041`
- Can be overridden with `VITE_API_BASE_URL` environment variable

## Troubleshooting

### "Failed to load players"
- Ensure backend is running (`dotnet run` from `backend/` folder)
- Check that `http://localhost:5041/api/players` is accessible
- Check backend console for errors

### Recommendations show only fallback content
- This is **expected** if `Claude__ApiKey` is not set
- The deterministic fallback ensures reliable responses
- To use live Claude: set `export Claude__ApiKey=sk-...` before running the backend

### Port conflicts
- Frontend defaults to `http://localhost:5173` (Vite)
- Backend defaults to `http://localhost:5041`
- If ports are in use, Vite will try the next available port (5174, 5175, etc.)
- Update `.env.local` to match the actual frontend URL

## Key Files

- **Backend API**: [backend/dotnet-test.sln](backend/dotnet-test.sln)
- **Frontend Entry**: [frontend/src/main.js](frontend/src/main.js)
- **API Client**: [frontend/src/api.js](frontend/src/api.js)
- **Routes**: [frontend/src/router.js](frontend/src/router.js)
- **Views**: `frontend/src/views/` (PlayersList, PlayerDetail, RecommendationOutput)

## Next Steps (Post-Demo)

1. **Production Build**: Fix the Tailwind build issue (CSS minification compatibility)
2. **CORS**: If frontend/backend are on different origins, enable CORS in backend
3. **Error UI**: Add better error boundary components for network failures
4. **Logging Dashboard**: Aggregate backend logs (optional: Seq, ELK, or file-based)
5. **Testing**: Add end-to-end tests with Cypress or Playwright
