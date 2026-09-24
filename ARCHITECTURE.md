# ARCHITECTURE.md — YouTube Viewer

Detailed architecture reference for the YouTube Viewer project. For progress and task status see `TASKS.md`; for agent workflow rules see `AGENTS.md`.

---

## 1. System Overview

The system is a three-stage pipeline that turns a list of YouTube video IDs into a browsable web UI. There is **no database** — plain JSON files are the persistence layer, and the API serves a fixed snapshot loaded into memory.

```
┌────────────────────┐   reads    ┌──────────────────────┐
│ input/videos.json  │ ─────────► │  YoutubeTest.Consumer│  YouTube Data API v3
│ (video IDs)        │            │  (console app)       │ ◄──────────────────
└────────────────────┘            └──────────┬───────────┘
                                             │ writes
                                             ▼
                                ┌──────────────────────┐
                                │ output/videos.json   │  (~9.9 MB, array of Video)
                                │ (full video data)    │
                                └──────────┬───────────┘
                                           │ loads ONCE at startup (in-memory)
                                           ▼
                                ┌──────────────────────┐        HTTP/JSON         ┌──────────────────────┐
                                │  YoutubeTest.Api     │ ◄─────────────────────── │    youtube-ui        │
                                │  (Minimal API)       │   GET /api/videos        │  (React SPA)         │
                                └──────────────────────┘   ?page=&pageSize=        └──────────────────────┘
```

**Data flow contract**

| Stage | Input | Output |
|-------|-------|--------|
| Consumer | `input/videos.json` (IDs) | `output/videos.json` (full data), `input/missing-videos.json` |
| API | `output/videos.json` (read once) | `GET /api/videos` → `{ items, page, pageSize, totalCount, totalPages }` |
| UI | API paged response | YouTube-style card grid with infinite scroll, EN/ES |

**Key decisions (fixed data):**
- The output JSON **never changes** during a session → the API loads it **once at startup** via an explicit `VideoStore.Load()` call; no reload, no file watching, no database.
- No search/filtering; the UI only lists and pages through videos.
- Card content is limited to **thumbnail + title + channel** (no views, no dates).

---

## 2. Technology Stack

### 2.1 Backend (`youtube.be/`)

| Technology | Version | Purpose |
|---|---|---|
| .NET | 10 | Runtime for Consumer + API |
| ASP.NET Core Minimal API | 10 | HTTP endpoints (no MVC controllers) |
| Serilog (+ `Serilog.AspNetCore`) | file sink | Structured logging to a **shared** log file |
| `Microsoft.Extensions.Options` | built-in | Configuration binding (`IOptions<T>`) |
| `System.Text.Json` | built-in | JSON serialization/deserialization |
| YouTube Data API v3 | REST | Source of video metadata (Consumer only) |

### 2.2 Frontend (`youtube-ui/`)

| Technology | Version | Purpose |
|---|---|---|
| Node.js / npm | 24.16 / 12 | Toolchain |
| Vite | 8.3 | Dev server + production bundler |
| React | 19.3 | UI library |
| TypeScript | ~6.0 (strict) | Type safety |
| **PrimeReact** | **10.9.9 (MIT)** | Components (`Button`, `ProgressSpinner`) + theme |
| PrimeIcons | 7 (MIT) | Icon font (`pi pi-play`, etc.) |
| PrimeFlex | 4 (MIT) | Utility CSS |
| i18next + react-i18next | 26 / 17 | EN/ES internationalization |
| oxlint | — | Linting |

> **Licensing note:** PrimeReact **v11+** is under the commercial *PrimeUI License* (banner "Invalid PrimeUI License" without a key). The project is intentionally **pinned to v10.9.9**, the last MIT major. Docs: https://v10.primereact.org/

### 2.3 Storage format

- **No database.** Files under `youtube.be/` (both git-ignored):
  - `input/videos.json` — array of video ID strings (input)
  - `input/missing-videos.json` — IDs the YouTube API did not return
  - `output/videos.json` — array of full `Video` objects (source of truth for the API)
  - `logs/youtubetest-*.txt` — **shared** Serilog file (Consumer + API, daily rolling, 7 retained)

---

## 3. Solution Layout

```
YoutubeTest/
├── TASKS.md                  # Source of truth: progress, decisions, notes
├── AGENTS.md                 # Agent workflow rules (links here)
├── ARCHITECTURE.md           # This document
├── youtube.be/               # .NET solution
│   ├── YoutubeTest.sln
│   ├── YoutubeTest.Shared/   # Shared models + helpers (referenced by Consumer & API)
│   │   ├── Models/           # Video + nested DTOs (snippet, thumbnails, stats…)
│   │   ├── Extensions/PathExtensions.cs
│   │   └── LogConstants.cs
│   ├── YoutubeTest.Consumer/ # Console app: YouTube fetcher pipeline
│   │   ├── Program.cs        # Composition root
│   │   ├── Models/AppSettings.cs
│   │   ├── Extensions/       # ServiceCollectionExtensions, PathExtensions (project-relative)
│   │   ├── Handlers/YouTubeAuthHandler.cs
│   │   └── Services/         # BatchProcessor, YouTubeFetcher, JsonOutputWriter
│   └── YoutubeTest.Api/      # Web API: serves the output JSON
│       ├── Program.cs        # Composition root + pipeline + endpoint
│       ├── Models/           # ApiSettings, PagedVideosResponse
│       ├── Services/VideoStore.cs
│       └── (no Controllers — Minimal API)
└── youtube-ui/               # React SPA
    ├── index.html, vite.config.ts, tsconfig*.json
    ├── .env / .env.example   # VITE_API_URL (git-ignored / tracked template)
    └── src/
        ├── main.tsx          # PrimeReactProvider + theme CSS + i18n bootstrap
        ├── App.tsx           # Layout: header (logo + language switcher) + main
        ├── types/video.ts    # Null-safe TS interfaces mirroring the API
        ├── services/videoService.ts
        ├── hooks/useInfiniteVideos.ts
        ├── i18n/             # index.ts + en.ts + es.ts
        └── components/       # VideoCard, VideoGrid, LanguageSwitcher (+ .css)
```

---

## 4. YoutubeTest.Shared (class library)

Referenced by **both** Consumer and API — guarantees one wire contract.

- **`Models/Video.cs`** (+ `VideoSnippet`, `ThumbnailSet`, `Thumbnail`, `ContentDetails`, `VideoStatus`, `VideoStatistics`, `PageInfo`, `YouTubeVideoListResponse`, …): POCOs with `[JsonPropertyName]` attributes so **camelCase JSON** (YouTube's format and the Consumer's output) binds in both directions regardless of serializer policy.
- **`FlexibleBoolConverter`**: tolerant `JsonConverter` for booleans that arrive as strings/numbers from the YouTube API.
- **`Extensions/PathExtensions.cs`**:
  - `ResolveProjectPath()` — resolves relative paths against the project directory (walks up until it finds a `*.csproj`).
  - `ResolveOutsideProjectPath()` — resolves relative paths against the **parent of the project** (i.e. `youtube.be/`), used for `output/`, `input/`, `logs/` so artifacts are shared between projects and outside any single project's folder.
- **`LogConstants.cs`**: single definition of log directory, file name prefix and output template — enforces the *"shared Serilog log file"* rule.

---

## 5. YoutubeTest.Consumer (console app)

**Responsibility:** read video IDs → call YouTube Data API v3 in batches of ≤50 → stream enriched video data to `output/videos.json` → report missing IDs.

### 5.1 Composition root (`Program.cs`)

Follows the *generic host* pattern without `Host.CreateDefaultBuilder` — a manual `ServiceCollection`:

1. Bootstraps **Serilog** immediately (`Log.Logger`) so failures during composition are captured: file sink (`youtube.be/logs/`, daily rolling, retain 7) + console sink.
2. Builds `IConfiguration` from **User Secrets only** (`Token`, `InputPath`, `OutputPath`, `YouTubeBaseUrl`, `BatchSize` — the API key never hits the repo).
3. Registers services and runs `BatchProcessor.ProcessAsync()` inside `try/catch` with `Log.Fatal` + `return 1` on failure (`finally Log.CloseAndFlush()`).

### 5.2 Pipeline (per run)

```
LoadVideoIdsAsync ──► FetchBatchesAsync (IAsyncEnumerable) ──► WriteBatchesAsync ──► SaveMissingVideosAsync
     (consumer)              (producer)                         (consumer)              (finalizer)
```

- **`BatchProcessor`** — orchestrator/façade:
  - Loads IDs from `input/videos.json` (`JsonDocument`, tolerant of string arrays).
  - Splits with `Enumerable.Chunk(BatchSize)` (default 50 = YouTube's hard limit per `videos.list` call).
  - Produces batches lazily via `IAsyncEnumerable<List<Video>>` + `yield return` (`[EnumeratorCancellation]`), so memory is bounded to one batch.
  - Error policy: a failed batch is **logged and skipped** (its IDs go to `missingIds`); a partial batch diffs received vs requested IDs. The run never aborts for a single bad batch.
- **`YouTubeFetcher`** — typed HTTP client consumer; builds `videos?part=snippet,contentDetails,status,statistics&id=…`, deserializes streaming from the response `Stream` into `YouTubeVideoListResponse`.
- **`JsonOutputWriter`** — true streaming writer: `File.Create` → `Utf8JsonWriter` (indented) writes each `Video` as it arrives and `Flush()`es per batch; closes the array at the end. Output file is never fully materialized in memory.

### 5.3 HTTP client best practices

```
AddHttpClient<YouTubeFetcher>            ← typed client (IHttpClientFactory)
   ├── BaseAddress / Timeout from IOptions<AppSettings>
   ├── .AddHttpMessageHandler(_ => new YouTubeAuthHandler(token))   ← DelegatingHandler
   └── .ConfigurePrimaryHttpMessageHandler(() => SocketsHttpHandler { PooledConnectionLifetime = 5 min })
```

- **Typed client** (`YouTubeFetcher` receives `HttpClient` ctor-injected) — no `IHttpClientFactory` lookups by name.
- **`YouTubeAuthHandler : DelegatingHandler`** — attaches `Authorization: Bearer <token>` to every outgoing request; token comes from User Secrets.
- **Pooled connection lifetime** avoids DNS pinning issues on long-lived processes.

### 5.4 Configuration

`AddAppSettings()` extension: `AddOptions<AppSettings>().Bind(configuration)` + **fluent `.Validate()`** rules that fail fast at startup with actionable messages (run `dotnet user-secrets set …`), `.PostConfigure()` to resolve paths through `PathExtensions`, and `.ValidateOnStart()`.

---

## 6. YoutubeTest.Api (Minimal API)

**Responsibility:** serve `output/videos.json` as a paginated, in-memory REST endpoint.

### 6.1 Lifecycle & data loading

```
Program.cs
  ├── Log.Logger = … (same Serilog config/path as Consumer — LogConstants)
  ├── builder.Host.UseSerilog()
  ├── services.Configure<ApiSettings>(…)      ← OutputPath from appsettings.json
  ├── services.AddSingleton<VideoStore>()
  ├── app = builder.Build()
  ├── var store = app.Services.GetRequiredService<VideoStore>();
  │   store.Load();                           ← EXPLICIT, ONCE, FAIL-FAST (see 6.2)
  ├── pipeline: UseHttpsRedirection → UseCors("ViteDev")
  └── MapGet("/api/videos", …) → app.Run()
```

### 6.2 `VideoStore` — in-memory repository

- Registered as a **singleton**; the instance lives for the process lifetime.
- **Light constructor** (only `IOptions<ApiSettings>` + `ILogger<VideoStore>`) — *no I/O in the constructor*.
- **`Load()`** is an explicit method invoked once from `Program.cs` after `Build()`:
  - Resolves `OutputPath` (`output/videos.json` → `youtube.be/output/videos.json` via `ResolveOutsideProjectPath()`).
  - Throws actionable exceptions if config is missing, the file does not exist, JSON is invalid, or I/O fails → **process exits before serving traffic** (fail-fast).
  - `IsLoaded` guard makes `Load()` **idempotent**; on success it logs `Loaded {VideoCount} videos from {VideoPath}`.
- `Videos` (`IReadOnlyList<Video>`) throws a clear `InvalidOperationException` if read before `Load()` — the invariant "singleton is always loaded when serving" is enforced by the startup call order.

### 6.3 Endpoint contract

```
GET /api/videos?page=1&pageSize=24
→ 200 { "items": Video[], "page": 1, "pageSize": 24, "totalCount": N, "totalPages": M }
```

- **Parameter clamping:** `page < 1 → 1`; `pageSize < 1 → 24` (default); `pageSize > 100 → 100`.
- **Out-of-range page** → `200` with empty `items` and correct totals (no 404; the UI stops when `page >= totalPages`).
- Implementation: LINQ `Skip/Take` over the in-memory list; `totalPages = ceil(totalCount / pageSize)`.
- Response DTO: `record PagedVideosResponse(...)` — immutable positional record.
- **No search** (`?q=` removed from scope by decision).

### 6.4 CORS

Policy `"ViteDev"`: single origin `http://localhost:5173` (Vite dev server), any method/header. Applied with `app.UseCors("ViteDev")` before endpoints.

### 6.5 Configuration

`appsettings.json` → `ApiSettings { OutputPath }` (relative, resolved to `youtube.be/`). No secrets in the API — it never talks to YouTube.

---

## 7. youtube-ui (React SPA)

**Responsibility:** display the video list as a YouTube-like grid with infinite scroll and EN/ES UI.

### 7.1 Bootstrap (`main.tsx`)

```tsx
<StrictMode>
  <PrimeReactProvider>        ← primereact/api (v10)
    <App />
  </PrimeReactProvider>
</StrictMode>
```

CSS imports (order matters): `primereact.min.css` → theme `lara-light-blue/theme.css` → `primeicons` → `primeflex` → app styles. i18n is initialized on import (`import './i18n'`).

### 7.2 Layered structure

| Layer | File(s) | Role |
|---|---|---|
| Presentation | `App.tsx`, `components/*` | Layout + rendering only; no fetch logic |
| State/behavior | `hooks/useInfiniteVideos.ts` | Owns paging state, fetch orchestration, IntersectionObserver |
| Service | `services/videoService.ts` | Single `fetchVideos(page, pageSize, signal)`; base URL from `VITE_API_URL` (default `http://localhost:5063`); throws on `!ok` |
| Contract | `types/video.ts` | Interfaces mirroring API JSON; **all nested fields optional/nullable** |
| i18n | `i18n/{index,en,es}.ts` | All user-facing strings; browser-language default; persisted |

### 7.3 Infinite scroll (no paginator component)

`useInfiniteVideos()` custom hook:

- State: `items`, `page`, `totalPages`, `loading` (initial), `loadingMore`, `error ('initial' | 'more' | null)`, `hasMore`.
- Initial load on mount with **`AbortController`** (canceled on unmount / StrictMode re-run).
- A **sentinel `<div>`** at the bottom of the grid is observed with `IntersectionObserver` (`rootMargin: '300px'` pre-fetches ahead of the viewport); when visible and `hasMore`, `loadMore()` fetches `page + 1` and **appends** items.
- Stops when `page >= totalPages` (driven by the API's `totalPages`).
- Errors: initial → full-panel error + retry (re-trigger mount effect via `reloadToken`); load-more → inline error + retry without losing already-loaded items.

### 7.4 Components

- **`VideoCard`** — *defensive rendering*: picks the best thumbnail (`maxres → standard → high → medium → default`), falls back to a placeholder tile with `pi pi-play`; title/channel fall back to **translated** strings (`video.noTitle`, `video.unknownChannel`). Content: thumbnail + title + channel only. `loading="lazy"` on images.
- **`VideoGrid`** — responsive CSS grid (4 / 3 / 2 / 1 columns at desktop / tablet / ≤767px / ≤479px); renders cards, spinner states, inline load-more error, end-of-list message, and the sentinel. Handles missing/duplicate IDs (`videoKey` fallback).
- **`LanguageSwitcher`** — plain buttons (no PrimeReact dependency) toggling `en`/`es`; `aria-pressed` for accessibility.
- **`App`** — YouTube-like chrome: white sticky header with CSS play-mark logo + wordmark, switcher on the right; main area `#f9f9f9`.

### 7.5 i18n

- Init: `i18n.use(initReactI18next).init({ resources: { en, es }, lng: detectLanguage(), fallbackLng: 'en' })`.
- `detectLanguage()`: `localStorage['youtube-ui-lang']` → else `navigator.language` startsWith `es` → `es`, else `en`.
- `languageChanged` listener persists the choice and sets `<html lang>`.
- Spanish translations live only in `i18n/es.ts` (data, not code — per AGENTS.md English-only-code rule).

### 7.6 Styling conventions

- Plain CSS files per component with **BEM-like naming**: `.video-card`, `.video-card__thumbnail`, `.video-card__title`, modifiers `--active`.
- YouTube-like look: light header, Roboto/system font stack, 12px thumbnail radius, hover elevation, 2-line title clamp.

---

## 8. Cross-Cutting Concerns

### 8.1 Configuration & secrets

| App | Source | Keys |
|---|---|---|
| Consumer | **User Secrets** (`dotnet user-secrets`) | `Token` (YouTube API key), `InputPath`, `OutputPath`, `YouTubeBaseUrl`, `BatchSize` |
| API | `appsettings.json` | `OutputPath` (non-secret) |
| UI | `.env` / `.env.example` (git-ignored / template) | `VITE_API_URL` |

Git-ignored: `TASKS.md`, `AGENTS.md` (agent files), `youtube.be/input/`, `youtube.be/output/`, `logs/`, `bin/`, `obj/`, `node_modules/`, `dist/`, `.env`.

### 8.2 Logging

- **Rule:** Consumer and API write to the **same** Serilog file (`youtube.be/logs/youtubetest-*.txt`) using `LogConstants` — one place to inspect the whole system.
- Consumer: `LoggerConfiguration` file + console sinks, bootstrapped *before* DI so composition errors are logged; `Log.Fatal` on unhandled, `Log.CloseAndFlush()` in `finally`.
- API: identical file-sink configuration; `builder.Host.UseSerilog()` for framework logs; startup log line reports video count.
- Structured messages with named holes (`{VideoCount}`, `{BatchCount}`).

### 8.3 Error handling strategy

| Layer | Strategy |
|---|---|
| Consumer config | Fluent validation → fail at startup with actionable message |
| Consumer runtime | Per-batch try/catch: skip + record missing IDs; whole-run try/catch → `Log.Fatal`, exit code 1 |
| API startup | `VideoStore.Load()` throws → process exits before serving (fail-fast) |
| API request | Clamping instead of 400s; out-of-range page → empty 200 |
| UI service | `!response.ok` → throw `Error` with status |
| UI render | Null-safe types + fallbacks (never crash on missing snippet/thumbnail) |
| UI async | Distinguish initial vs load-more errors; AbortController prevents state updates after unmount |

### 8.4 Cancellation / abort

- .NET: `CancellationToken` flows Program → `BatchProcessor` → `FetchBatchesAsync` (`[EnumeratorCancellation]`) → `HttpClient` → `Utf8JsonWriter`.
- Browser: `AbortSignal` from `AbortController` in the fetch service; observer disconnected on cleanup.

---

## 9. Design Patterns & Techniques Used

### Backend (.NET)

| Pattern / technique | Where | Notes |
|---|---|---|
| **Composition Root** | `Program.cs` (Consumer & API) | All wiring at startup; services never new up dependencies ad hoc |
| **Dependency Injection** (constructor injection) | All services; primary constructors used for brevity | `BatchProcessor`, `YouTubeFetcher`, `VideoStore`… |
| **Options pattern** (`IOptions<T>` + validation) | `AppSettings`, `ApiSettings` | Bind → Validate → PostConfigure → `ValidateOnStart` |
| **Typed HttpClient** | `AddHttpClient<YouTubeFetcher>` | Strongly typed client over `IHttpClientFactory` |
| **Delegating Handler** (Chain of Responsibility) | `YouTubeAuthHandler` | Inserts Bearer token; composable in the message pipeline |
| **Singleton** | `VideoStore` | One in-memory dataset per process |
| **Factory Method** | `VideoStore.Load()` (static factory-style method) | Encapsulates construction/loading; light ctor, explicit load |
| **Idempotent operation** | `Load()` `IsLoaded` guard | Safe to call twice; loads only once |
| **Repository** (in-memory) | `VideoStore.Videos` | Endpoint depends on an abstraction of "where videos come from", not on files |
| **Façade / Orchestrator** | `BatchProcessor` | One entry point (`ProcessAsync`) hiding load → fetch → write → missing-report |
| **Producer–Consumer** | `IAsyncEnumerable` batches → `Utf8JsonWriter` | Streaming pipeline; memory bounded per batch (backpressure by lazy enumeration) |
| **Iterator** | `yield return` in `FetchBatchesAsync` | Lazy batch generation |
| **Fail-fast** | Config validation; `VideoStore.Load()` at startup | Broken state never serves traffic |
| **Fault isolation** | Per-batch catch in `FetchBatchesAsync` | One failed batch doesn't kill the run; IDs recorded as missing |
| **DTO / shared contract** | `YoutubeTest.Shared.Models` | Single wire model for Writer + API |
| **Tolerant deserialization** | `FlexibleBoolConverter`, case-insensitive options | Handles YouTube's inconsistent field types |
| **Path resolution strategy** | `PathExtensions` (project vs outside-project) | Keeps artifacts (`input/`, `output/`, `logs/`) shared under `youtube.be/` |
| **Record types** | `PagedVideosResponse` | Immutable response DTO |

### Frontend (React/TS)

| Pattern / technique | Where | Notes |
|---|---|---|
| **Custom Hook** | `useInfiniteVideos` | Encapsulates state + side effects; reusable, testable |
| **Service layer** | `videoService.ts` | UI never calls `fetch` directly; single place for base URL/errors |
| **Observer** | `IntersectionObserver` on sentinel | DOM visibility drives data loading (scroll = event source) |
| **Optimistic pagination state machine** | hook's `loading`/`loadingMore`/`error` flags | Prevents duplicate fetches; distinguishes first load vs append |
| **Abort on unmount** | `AbortController` in hook + service | Avoids memory leaks and setState-after-unmount (StrictMode-safe) |
| **Defensive (null-safe) typing** | `types/video.ts` optional fields | Model mirrors a possibly-partial API payload |
| **Progressive fallback rendering** | `VideoCard` thumbnail preference list + i18n fallbacks | Degrades gracefully instead of crashing |
| **Controlled i18n singleton** | `i18n/index.ts` | Detection → persistence → `<html lang>` as one concern |
| **BEM-like CSS** | component `.css` files | Scoped-by-convention styling without a CSS-in-JS runtime |
| **Provider** | `PrimeReactProvider`, `initReactI18next` | Context-based cross-cutting UI/config |
| **Environment-driven config** | `import.meta.env.VITE_API_URL` | 12-factor style config per environment |

### General

- **Separation of concerns by project/folder:** Shared (contract) | Consumer (producer) | API (server) | UI (client).
- **Single source of truth:** `TASKS.md` for state/decisions; `ARCHITECTURE.md` (this file) for structure/patterns; JSON files for data.
- **English-only code/docs rule** with i18n resource files as the sole location of Spanish UI copy.

---

## 10. API Contract Reference (UI ↔ API)

```http
GET /api/videos?page=1&pageSize=24
Origin: http://localhost:5173        (CORS-enforced)
```

```json
{
  "items": [
    {
      "kind": "youtube#video",
      "etag": "…",
      "id": "dQw4w9WgXcQ",
      "snippet": {
        "title": "…",
        "channelTitle": "…",
        "thumbnails": {
          "maxres": { "url": "https://…", "width": 1280, "height": 720 },
          "high":   { "url": "https://…", "width": 480,  "height": 360 }
        }
      },
      "statistics": { "viewCount": "…" },
      "status": { "privacyStatus": "public" },
      "contentDetails": { "duration": "PT…" }
    }
  ],
  "page": 1,
  "pageSize": 24,
  "totalCount": 500,
  "totalPages": 21
}
```

**UI usage rules:** append pages until `page >= totalPages`; treat every nested object as nullable; display only thumbnail/title/channel.

---

## 11. Runbook (manual verification)

```bash
# 1. Backend API (loads output JSON at startup, listens on 5063)
cd youtube.be && dotnet run --project YoutubeTest.Api

# 2. Frontend (5173, talks to API via VITE_API_URL)
cd youtube-ui && npm run dev

# Compile-only checks (CI policy — do not auto-run apps)
cd youtube.be && dotnet build
cd youtube-ui && npm run build
```

Optional (regenerates `output/videos.json`): `dotnet run --project YoutubeTest.Consumer` — requires User Secrets (`Token`, `InputPath`, `OutputPath`).
