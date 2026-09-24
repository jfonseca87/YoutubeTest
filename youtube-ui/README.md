# YouTube Viewer — Frontend

React frontend for the YouTube Viewer project. Displays a paginated list of videos from the local .NET API with infinite scroll and EN/ES translations.

## Stack

- Vite 8 + React 19 + TypeScript
- PrimeReact 11 (Aura theme preset), PrimeIcons, PrimeFlex
- i18next + react-i18next (EN/ES, browser default, persisted in localStorage)

## Setup

```bash
npm install
cp .env.example .env
npm run build   # compile check (tsc + vite)
npm run dev     # local development (default port 5173)
```

## Environment

| Variable | Description | Default |
|----------|-------------|---------|
| `VITE_API_URL` | Base URL of the .NET API | `http://localhost:5063` |
| `VITE_PRIMEUI_LICENSE` | Optional PrimeReact v11 styled-mode license key | — |

The API must allow CORS from `http://localhost:5173`.

## Notes

- Cards show thumbnail, title, and channel only (no views, no dates).
- Infinite scroll loads pages of 24 videos via IntersectionObserver.
- PrimeReact v11 applies the Aura light theme through `PrimeReactProvider`; without a license key the library logs a warning and shows a small license banner at runtime (get a key at primeui.dev to remove it).
