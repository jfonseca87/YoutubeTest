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
├── ARCHITECTURE.md    # Detailed architecture, tech stack & design patterns
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
| 1.12 | Init local git + `.gitignore` for be and ui | ✅ Done | Excludes for .NET and React |

### Phase 2: Shared Model

| # | Task | Status | Notes |
|---|------|--------|-------|
| 2.1 | Create shared model in `YoutubeTest.Shared` | ✅ Done | Modelo compartido Consumer + API |

### Phase 3: .NET Consumer Implementation

| # | Task | Status | Notes |
|---|------|--------|-------|
| 3.1 | Implement Serilog logging (file) + HttpClient best practices in Consumer | ✅ Done | Shared log, console sink, typed client + Bearer auth handler |
| 3.2 | Create `Services/YouTubeFetcher.cs` | ✅ Done | YouTube API v3 calls |
| 3.3 | Create `AppSettings` model + load from User Secrets | ✅ Done | YouTubeBaseUrl, Token, InputPath, OutputPath, BatchSize |
| 3.4 | Implement batch processing logic | ✅ Done | Groups of 50 videos |
| 3.5 | Implement JSON output writer | ✅ Done | Save enriched data |
| 3.6 | Apply `IOptions` pattern for settings | ✅ Done | Bind + validation + PostConfigure |
| 3.7 | Create `PathExtensions` for path resolution | ✅ Done | Input → project dir, output/logs → outside Consumer |
| 3.8 | Refactor to streaming with `IAsyncEnumerable`/`yield` | ✅ Done | Memory bounded per batch |
| 3.9 | Accumulate missing video IDs across batches | ✅ Done | Written once to `missing-videos.json` |
| 3.10 | Shared Serilog log path outside Consumer | ✅ Done | `youtube.be/logs/` |

### Phase 4: .NET API Implementation

| # | Task | Status | Notes |
|---|------|--------|-------|
| 4.1 | Implement Serilog logging (file) in API | ✅ Done | Shared log file with Consumer (`youtube.be/logs/`) via Serilog.AspNetCore 10 + LogConstants |
| 4.2 | Create `Services/VideoStore.cs` | ✅ Done | Singleton with explicit `Load()` invoked once in `Program.cs` after `Build()` (fail-fast at startup); `IsLoaded` guard makes it idempotent; clear error if missing/invalid |
| 4.3 | Create Minimal API endpoint in `Program.cs` | ✅ Done | GET /api/videos (weather forecast template removed) |
| 4.4 | Configure CORS in `Program.cs` | ✅ Done | Allow `http://localhost:5173` (Vite) |
| 4.5 | Add pagination support | ✅ Done | `?page=1&pageSize=24` → `{ items, page, pageSize, totalCount, totalPages }`; pageSize clamp 1–100 |
| 4.6 | ~~Add search/filter support~~ | ❌ Removed | No search — listing only |

### Phase 5: React Frontend Setup

| # | Task | Status | Notes |
|---|------|--------|-------|
| 5.1 | Create Vite + React + TypeScript project | ✅ Done | In `youtube-ui/` (existing `.gitignore` preserved); Node 24.16 + Vite 8.3 + React 19.3 |
| 5.2 | Install PrimeReact, PrimeIcons, PrimeFlex | ✅ Done | PrimeReact **10.9.9 (MIT)**, PrimeIcons 7 (MIT), PrimeFlex 4; v11+ is PrimeUI-licensed — stay on v10 |
| 5.3 | Create `types/video.ts` | ✅ Done | TypeScript interfaces, all nested fields optional/nullable |
| 5.4 | Create `services/videoService.ts` | ✅ Done | `fetchVideos(page, pageSize, signal)`; `.env` + `.env.example` with `VITE_API_URL=http://localhost:5063` |
| 5.5 | Setup i18n (EN/ES) | ✅ Done | `react-i18next` + `i18next`; `en`/`es` resources; default = browser language; persisted in localStorage key `youtube-ui-lang` |

### Phase 6: React UI Components

| # | Task | Status | Notes |
|---|------|--------|-------|
| 6.1 | Create `components/VideoCard.tsx` | ✅ Done | Thumbnail (maxres→…→default→placeholder) + title (2-line clamp) + channel; null-safe i18n fallbacks; hover elevation |
| 6.2 | Create `components/VideoGrid.tsx` | ✅ Done | Responsive CSS grid: 4 / 3 / 2 / 1 cols (desktop/tablet/mobile) + sentinel + spinner + end-of-list |
| 6.3 | Implement infinite scroll | ✅ Done | `useInfiniteVideos` hook: IntersectionObserver sentinel (rootMargin 300px), pageSize 24, AbortController, initial/load-more error + retry |
| 6.4 | Create `components/LanguageSwitcher.tsx` | ✅ Done | EN/ES segmented toggle; persists via i18n `languageChanged` → localStorage |
| 6.5 | Create `App.tsx` layout | ✅ Done | White sticky header (CSS play-mark logo + wordmark, switcher right) + `#f9f9f9` main with grid |

### Phase 7: Integration & Polish

| # | Task | Status | Notes |
|---|------|--------|-------|
| 7.1 | Connect React to API | ✅ Done | `VITE_API_URL` in `.env`/`.env.example` (CORS itself is API-side task 4.4) |
| 7.2 | Style adjustments | ✅ Done | YouTube-like: light header, Roboto, 12px thumbnail radius, hover shadows, gray main bg |
| 7.3 | Error handling | ✅ Done | Loading spinner (PrimeReact `ProgressSpinnerRoot`), error + translated retry (initial & load-more), empty state, null-safe model throughout |
| 7.4 | Test end-to-end flow | ⏳ Pending | Consumer output JSON → API → UI (infinite scroll + i18n) |

---

## Rules
- **Shared Serilog log file:** The Serilog log file must be shared between Consumer and API (same path/file).
- **English only for code:** Identifiers, code comments and log messages in English. UI user-facing copy lives in i18n resources (`en`/`es`) — Spanish translations there are data, not code.
- **Verification:** Before marking tasks as Done: run `dotnet build` (solution) and `npm run build` (youtube-ui) — compile only.
- **Do NOT run the API or UI** — the user performs end-to-end testing manually and reports back.

---

## Notes
- YouTube API v3 max per request: 50 videos (batch size)
- API Key provided by user (not committed to repo)
- Input JSON: enriched format with video IDs (user-provided)
- Output JSON: full video data from YouTube API — exists at `youtube.be/output/videos.json` (~9.9 MB)
- No database, no authentication required
- **API data:** loaded into memory ONCE at startup via explicit `VideoStore.Load()` call in `Program.cs` (fixed data, never reloaded)
- **API endpoint:** `GET /api/videos?page=1&pageSize=24` (no search); response `{ items, page, pageSize, totalCount, totalPages }`
- **API URL:** `http://localhost:5063` (http profile); CORS origin: `http://localhost:5173`
- **UI card content:** thumbnail + title + channel only (no views, no date)
- **UI pattern:** infinite scroll (IntersectionObserver), pageSize 24
- **i18n:** EN/ES via `react-i18next`; default browser language; persisted in localStorage
- **Stack versions:** Node 24.16, npm 12, Vite 8.3, React 19.3, TypeScript, **PrimeReact 10.9.9 (MIT)**, PrimeIcons 7 (MIT), PrimeFlex 4 (MIT), react-i18next 17, i18next 26
- **PrimeReact:** pinned to v10 (last MIT major). v11+ uses the commercial PrimeUI license (banner "Invalid PrimeUI License" without a key). Docs v10: https://v10.primereact.org/ · Theming: `primereact/resources/themes/lara-light-blue/theme.css` + `primereact/resources/primereact.min.css`; provider from `primereact/api`
