# AGENTS.md - YouTube Viewer

## IMPORTANT: Always consult TASKS.md

Before starting any work on this project, you **MUST** read and understand the `TASKS.md` file located in the root of this project (`YoutubeTest/TASKS.md`). 

`TASKS.md` is the **single source of truth** for:
- Project structure and architecture
- Current progress and completed tasks
- Pending tasks and their dependencies
- Technical decisions and notes

For a **detailed architecture reference** (components, data flow, technologies and design patterns of the Consumer, API and UI) read `ARCHITECTURE.md`.

## Workflow

1. **Read `TASKS.md` first** to understand current state
2. **Consult `ARCHITECTURE.md`** when you need structural/pattern details before changing code
3. **Pick the next pending task** (follow the order)
4. **Update `TASKS.md`** status when task is completed
5. **Never create files or folders outside** the `YoutubeTest/` directory
6. **Follow the existing architecture** — do not introduce new patterns without updating TASKS.md (and ARCHITECTURE.md if the structure changes)
7. **English only for documentation** — any additional document (markdown, comments in docs, etc.) must be written in English
8. **English only for code** — all generated code (identifiers, strings, comments, log messages, error messages) must be written in English

## Project Structure

```
YoutubeTest/
├── TASKS.md            ← READ THIS FIRST
├── AGENTS.md           ← Agent instructions
├── ARCHITECTURE.md     ← Detailed architecture, tech stack & design patterns
├── youtube.be/         ← .NET solution (Consumer + API)
└── youtube-ui/         ← React frontend (Vite + PrimeReact)
```

## Tech Stack
- .NET 10 (Consumer console app + Web API)
- React 19+ with Vite, PrimeReact, PrimeIcons, PrimeFlex
- TypeScript
- YouTube Data API v3
- JSON file storage (no database)
- No authentication required
