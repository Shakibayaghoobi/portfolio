# Portfolio — Nima Adlimoghadam

A bilingual (English / Persian) architecture portfolio with a public site and an admin site. Everything the visitor sees (texts, sections, projects, images, SEO) is editable from admin.

| Part | Tech | Folder | Dev URL |
|---|---|---|---|
| Public site | Angular 22, SSR + prerender | `frontend/projects/client` | http://localhost:4200 |
| Admin site | Angular 22 (SPA, noindex) | `frontend/projects/admin` | http://localhost:4300 |
| Shared code | Angular library (models, API client) | `frontend/projects/shared` | — |
| API | ASP.NET Core 10, EF Core | `backend/src/Portfolio.Api` | http://localhost:5128 |
| Database | SQL Server | `docker-compose.yml` | localhost:1433 |

Full design: [docs/IMPLEMENTATION_GUIDE.md](docs/IMPLEMENTATION_GUIDE.md).

## Prerequisites
Node 22+, .NET SDK 10, Docker (or any SQL Server 2019+).

## Run locally
```bash
# 1. database
docker compose up -d

# 2. API (applies migrations and seeds the admin user in Development)
cd backend && dotnet run --project src/Portfolio.Api --launch-profile http
#    dev admin login: admin@example.com / ChangeMe!12345  (appsettings.Development.json)

# 3. frontends (new terminals); /api is proxied to the API
cd frontend && npm install
npm run start:client     # http://localhost:4200
npm run start:admin      # http://localhost:4300
```

## Build and test
```bash
cd backend && dotnet build && dotnet test
cd frontend && npm run build:all && npm run test:all
node frontend/dist/client/server/server.mjs   # run the SSR server
```

## Configuration
Set these via environment variables or user-secrets in any real environment:
`ConnectionStrings__Default`, `Jwt__Key` (32+ chars), `Admin__Email`, `Admin__Password` (first admin, seeded when none exists), `Cors__Origins__0`.

## Migrations
```bash
cd backend
dotnet tool restore
dotnet ef migrations add <Name> -p src/Portfolio.Infrastructure -s src/Portfolio.Api -o Migrations
dotnet ef database update -p src/Portfolio.Infrastructure -s src/Portfolio.Api
```

## Fonts and logo
Fonts are self-hosted from `frontend/shared-assets/fonts` (Barlow Semi Condensed and Vazirmatn as open-source stand-ins). To use the brand font, add its woff2 files there and follow `frontend/shared-assets/fonts/README.md`. The logo is `frontend/shared-assets/brand/logo.svg` (cropped from `Logo.pdf`; original kept in `docs/`).

## Status
Phases 0 and 1 of the roadmap are done. Phase 1 added: an idempotent content seeder (reference `content.json`, run automatically in Development; images are placeholders until Phase 2), public endpoints (`/api/public/content|site|projects|projects/{slug}|cv`), admin endpoints (`/api/admin/content|site|sections`), a public home page filled from the API, and admin editors for Texts, Site settings and Page sections. Backend tests use SQLite in memory, so they need no SQL Server.
