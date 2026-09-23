# AGENTS.md - YouTube Viewer

## IMPORTANT: Always consult TASKS.md

Before starting any work on this project, you **MUST** read and understand the `TASKS.md` file located in the root of this project (`YoutubeTest/TASKS.md`). 

`TASKS.md` is the **single source of truth** for:
- Project structure and architecture
- Current progress and completed tasks
- Pending tasks and their dependencies
- Technical decisions and notes

## Workflow

1. **Read `TASKS.md` first** to understand current state
2. **Pick the next pending task** (follow the order)
3. **Update `TASKS.md`** status when task is completed
4. **Never create files or folders outside** the `YoutubeTest/` directory
5. **Follow the existing architecture** — do not introduce new patterns without updating TASKS.md
6. **English only for documentation** — any additional document (markdown, comments in docs, etc.) must be written in English

## Project Structure

```
YoutubeTest/
├── TASKS.md          ← READ THIS FIRST
├── AGENTS.md         ← Agent instructions
├── youtube.be/       ← .NET solution (Consumer + API)
└── youtube-ui/       ← React frontend (Vite + PrimeReact)
```

## Tech Stack
- .NET 10 (Consumer console app + Web API)
- React 19+ with Vite, PrimeReact, PrimeIcons, PrimeFlex
- TypeScript
- YouTube Data API v3
- JSON file storage (no database)
- No authentication required
