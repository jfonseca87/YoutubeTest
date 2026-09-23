# TASKS.md - YouTube Viewer

## Project Overview
Aplicación para visualizar listado de videos de YouTube.
- **Stack:** .NET 10 + React (Vite + PrimeReact + TypeScript)
- **Almacenamiento:** Archivo JSON (sin base de datos)
- **Autenticación:** No requerida

## Structure
```
YoutubeTest/
├── TASKS.md
├── AGENTS.md
├── youtube.be/          # Solución .NET
│   ├── YoutubeTest.sln
│   ├── YoutubeTest.Shared/
│   ├── YoutubeTest.Consumer/
│   └── YoutubeTest.Api/
└── youtube-ui/          # React Frontend
```

---

## Tasks

### Phase 1: Project Scaffolding

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1.1 | Create `YoutubeTest/` folder | ✅ Done | Root folder |
| 1.2 | Create `TASKS.md` | ✅ Done | This file |
| 1.3 | Create `AGENTS.md` | ✅ Done | Agent instructions |
| 1.4 | Create `youtube.be/` folder | ✅ Done | .NET root |
| 1.5 | Create .NET solution `YoutubeTest.sln` | ✅ Done | `dotnet new sln` |
| 1.6 | Create `YoutubeTest.Consumer` project | ✅ Done | Console app |
| 1.7 | Create `YoutubeTest.Api` project | ✅ Done | Web API |
| 1.8 | Add projects to solution | ✅ Done | `dotnet sln add` |
| 1.9 | Create `youtube-ui/` folder | ✅ Done | React root |
| 1.10 | Create `YoutubeTest.Shared` class library | ✅ Done | Shared models/services |
| 1.11 | Add Shared reference to Consumer & API | ✅ Done | `dotnet add reference` |
| 1.12 | Init local git + `.gitignore` for be and ui | ⏳ Pending | Excludes for .NET and React |

### Phase 2: Shared Model

| # | Task | Status | Notes |
|---|------|--------|-------|
| 2.1 | Create shared model in `YoutubeTest.Shared` | ✅ Done | Modelo compartido Consumer + API |

### Phase 3: .NET Consumer Implementation

| # | Task | Status | Notes |
|---|------|--------|-------|
| 3.1 | Implement Serilog logging (file) + HttpClient best practices in Consumer | ✅ Done | Log archivo, IHttpClientFactory/typed client |
| 3.2 | Create `Services/YouTubeFetcher.cs` | ✅ Done | YouTube API v3 calls |
| 3.3 | Create `AppSettings` model + load from User Secrets | ✅ Done | YouTubeBaseUrl, Token, InputPath, OutputPath, BatchSize |
| 3.4 | Implement batch processing logic | ✅ Done | Groups of 50 videos |
| 3.5 | Implement JSON output writer | ✅ Done | Save enriched data |

### Phase 4: .NET API Implementation

| # | Task | Status | Notes |
|---|------|--------|-------|
| 4.1 | Implement Serilog logging (file) in API | ⏳ Pending | Log archivo en Api |
| 4.2 | Create `Services/VideoStore.cs` | ⏳ Pending | Load JSON, serve data |
| 4.3 | Create Minimal API endpoint in `Program.cs` | ⏳ Pending | GET /api/videos |
| 4.4 | Configure CORS in `Program.cs` | ⏳ Pending | Allow localhost:5173 |
| 4.5 | Add pagination support | ⏳ Pending | ?page=1&pageSize=20 |
| 4.6 | Add search/filter support | ⏳ Pending | ?q=term |

### Phase 5: React Frontend Setup

| # | Task | Status | Notes |
|---|------|--------|-------|
| 5.1 | Create Vite + React + TypeScript project | ⏳ Pending | `npm create vite@latest` |
| 5.2 | Install PrimeReact, PrimeIcons, PrimeFlex | ⏳ Pending | `npm install primereact primeicons primeflex` |
| 5.3 | Create `types/video.ts` | ⏳ Pending | TypeScript interfaces |
| 5.4 | Create `services/videoService.ts` | ⏳ Pending | API calls |

### Phase 6: React UI Components

| # | Task | Status | Notes |
|---|------|--------|-------|
| 6.1 | Create `components/VideoCard.tsx` | ⏳ Pending | YouTube-style card |
| 6.2 | Create `components/VideoGrid.tsx` | ⏳ Pending | Responsive grid layout |
| 6.3 | Implement pagination/scroll | ⏳ Pending | PrimeReact Paginator |
| 6.4 | Add search functionality | ⏳ Pending | Search bar component |
| 6.5 | Create `App.tsx` layout | ⏳ Pending | Main layout |

### Phase 7: Integration & Polish

| # | Task | Status | Notes |
|---|------|--------|-------|
| 7.1 | Connect React to API | ⏳ Pending | Proxy or direct call |
| 7.2 | Style adjustments | ⏳ Pending | YouTube-like appearance |
| 7.3 | Error handling | ⏳ Pending | Loading states, errors |
| 7.4 | Test end-to-end flow | ⏳ Pending | Consumer → API → UI |

---

## Rules
- **User approval required:** For each step/task, wait for the user's explicit approval before marking it as completed and moving to the next one.
- **Do not compile or run:** Do not execute `dotnet build/run` or any run commands; only write code and update TASKS.
- **Shared Serilog log file:** The Serilog log file must be shared between Consumer and API (same path/file).

---

## Notes
- YouTube API v3 max per request: 50 videos (batch size)
- API Key provided by user (not committed to repo)
- Input JSON: enriched format with video IDs (user-provided)
- Output JSON: full video data from YouTube API
- No database, no authentication required
