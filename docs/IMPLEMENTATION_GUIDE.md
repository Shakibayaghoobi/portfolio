# Implementation Guide

Bilingual (EN/FA, RTL) architecture portfolio. Two Angular apps (public client, admin), one ASP.NET Core API, SQL Server. **Every text, section and image on the public site is editable in admin.** Strong SEO is a hard requirement.

The visual design and content come from the reference design (dark "drawing sheet" look, amber accent `#f0c27a`, sections: hero, about with day/night image, works index with render↔sketch slider, line studies, practice/toolkit, film, recognition, path, contact) and the CV.

## 1. Architecture

```
Browser ──► Public site (Angular SSR/prerender, Node)  ──► API (ASP.NET Core) ──► SQL Server
Browser ──► Admin SPA (Angular, static, noindex)        ──► API  (JWT)            media files on disk/blob
```

- **client**: server-rendered + prerendered pages so crawlers get full HTML. The SSR server calls the API over the internal network; responses are cached.
- **admin**: static SPA behind login, `noindex`. Talks to `/api/admin/*`.
- **API**: public read endpoints (anonymous, cacheable) and admin write endpoints (JWT, role Admin). Serves media and generates `sitemap.xml`/`robots.txt` data.
- **Layering** (backend): `Domain` (entities) ← `Application` (use cases, DTOs, validators) ← `Infrastructure` (EF Core, storage) ← `Api` (controllers, auth). Keep controllers thin.

## 2. Repository layout

```
backend/
  Portfolio.sln, dotnet-tools.json (dotnet-ef)
  src/Portfolio.Domain          entities
  src/Portfolio.Application     use cases / DTOs (to be filled)
  src/Portfolio.Infrastructure  DbContext, migrations, storage
  src/Portfolio.Api             controllers, JWT auth, config
  tests/Portfolio.Tests         xUnit + WebApplicationFactory
frontend/                       Angular workspace (npm)
  projects/client               public site (SSR + prerender)
  projects/admin                admin SPA
  projects/shared               models, API client, shared UI (consumed by both)
  shared-assets/{fonts,brand}   served by both apps
  styles/                       design tokens, fonts, base styles (SCSS)
docs/  docker-compose.yml  README.md
```

## 3. Data model (SQL Server, EF Core)

Bilingual text is a complex type `LocalizedText {En, Fa}` stored as two columns (`Title_En`, `Title_Fa`) — queryable, no JSON parsing, and adding a language later is a migration.

| Table | Purpose / key fields |
|---|---|
| `ContentEntries` | Any copy by stable key (`nav.works`, `hero.tag`, `about.p1`, form labels…). `Key` unique, `Group`, `Value` (EN/FA). Rich text uses the same light markup as the design (`*emphasis*`, `<br>`), rendered safely. |
| `SiteSettings` | Site name, accent colour, contact email/phone/Instagram/address, hero/portrait/film/poster/logo media (`MediaAssetId`). |
| `Sections` | Key, `SortOrder`, `IsVisible` — admin reorders/hides page sections. |
| `SeoMetas` | Per route: title, description (EN/FA), OG image, canonical, `NoIndex`. |
| `MediaAssets` | File name, content type, size, width/height, `StoragePath`, `Alt` (EN/FA), folder. Variants (webp/avif, responsive widths) are generated on upload. |
| `Projects` / `ProjectImages` | Slug, number (A-01), title/type/note (EN/FA), order, published; images reference a render `MediaAsset` and a sketch `MediaAsset`, with order. |
| `Services`, `Awards`, `EducationItem`, `ExperienceItem`, `Skills`, `LanguageSkills` | CV-driven lists, each with `SortOrder`, bilingual text where relevant. |
| `ContactMessages` | Messages from the contact form (rate-limited), read flag. |
| `AdminUsers` | Email + ASP.NET Core `PasswordHasher` hash, last login. |

All entities have `CreatedUtc`/`UpdatedUtc`. Initial migration is committed; seed the first content from the reference `content.json` with an idempotent seeder (phase 2).

**Coverage rule:** a PR that adds anything visible to the public site must add the corresponding admin editor. No hard-coded copy in client templates except error fallbacks.

## 4. Media

- Upload to `POST /api/admin/media` (multipart, 20 MB limit, allow-list of image/video types, magic-byte check, random file names).
- Pipeline (ImageSharp or similar): store original, generate webp/avif at 480/768/1280/1920 widths, record intrinsic size, optional blurhash/LQIP.
- Storage behind `IMediaStorage` (disk under `storage/` in dev; Azure Blob/S3 or a mounted volume in prod). Public URLs: `/media/{id}/{width}.{ext}` with long immutable cache headers, served by the API or a CDN.
- Deleting a used asset is blocked (FKs are `Restrict`). Alt text is required (EN/FA) before publishing.

## 5. API design

Public (anonymous, `Cache-Control` + ETag, response compression):
- `GET /api/public/content` — all copy map `{key: {en, fa}}`
- `GET /api/public/site` — settings, sections (ordered, visible)
- `GET /api/public/projects`, `GET /api/public/projects/{slug}`
- `GET /api/public/cv` — services, awards, education, experience, skills, languages
- `GET /api/public/seo?route=/` — SEO meta for a route
- `GET /api/public/sitemap` — URLs + lastmod; `POST /api/public/contact` — rate-limited, validated
- `GET /api/health`

Admin (`[Authorize(Roles="Admin")]`, `/api/admin/*`): `auth/login|me|refresh`, and CRUD for `content`, `site`, `sections` (reorder), `projects` (+images, reorder), `services`, `awards`, `education`, `experience`, `skills`, `languages`, `seo`, `media`, `messages`, `account/password`.

Conventions: DTOs (never entities) over the wire; FluentValidation or DataAnnotations; RFC 7807 problem details; optimistic concurrency via `RowVersion` on editable entities; a "publish" step is optional (start with direct save, add draft/publish if wanted).

## 6. Admin authentication

- Phase 1 (done): email + password, ASP.NET `PasswordHasher`, short-lived JWT (60 min), role claim, first admin seeded from config.
- Harden: refresh tokens (rotating, httpOnly SameSite=Strict cookie) instead of `sessionStorage`; login rate limiting + lockout; password change; optional TOTP 2FA; security headers (CSP, HSTS); CORS limited to admin origin; admin served from a separate subdomain (`admin.example.com`); audit log of edits.
- Secrets only via env/user-secrets/key vault. Never commit `Jwt__Key`.

## 7. Admin UX

Left nav: Dashboard, Site & SEO, Page sections (drag to reorder, show/hide), Texts (grouped by section, EN | FA side by side, RTL input for FA), Projects (list → editor with image grid, render/sketch pairing, drag reorder), CV (services, awards, education, experience, skills, languages), Media library (grid, upload, alt text, usage), Messages. Live preview link opens the public page with `?preview=token` to show unpublished changes. Unsaved-change guard and optimistic UI with clear errors.

## 8. Public site

- Routes: `/` (single-page sections with anchors) plus real URLs for indexable depth: `/projects`, `/projects/:slug`, and language prefixes `/en/…`, `/fa/…` (Persian is RTL: `<html lang="fa" dir="rtl">`, logical CSS properties). Default language by prefix, with `hreflang` alternates and `x-default`.
- Components: header/nav + language switch, hero, about (day/night cross-fade), works index + gallery + render↔sketch slider, studies (SVG line drawings), practice/toolkit, film, recognition, path (timeline), contact form, footer. Respect `prefers-reduced-motion`.
- Data: route resolvers load from the API on the server; use Angular `TransferState` (automatic with `HttpClient` + hydration) to avoid refetching in the browser.
- Fonts self-hosted with `font-display: swap`, preloaded critical weight; brand font dropped into `shared-assets/fonts`.

## 9. SEO plan

1. **Rendering**: Angular SSR with hydration; prerender static routes at build (`RenderMode.Prerender`), SSR for data-driven routes (projects) or prerender them via `getPrerenderParams` fetching slugs. Incremental hydration/`@defer` for below-the-fold galleries.
2. **Meta**: per-route `title`, `description`, canonical, Open Graph, Twitter cards from `SeoMetas` (done in `SeoService`; wire to API). Unique and ≤60/160 chars; admin shows live length counters and a SERP preview.
3. **Structured data (JSON-LD)**: `Person` + `Architect`/`ProfessionalService` (name, jobTitle, sameAs Instagram, address Mashhad, telephone), `WebSite`, `BreadcrumbList`, `CreativeWork`/`ImageGallery` per project, `ScholarlyArticle` for the research item. Generated from DB content.
4. **International**: `hreflang` en/fa/x-default, `og:locale`, language-prefixed URLs, translated slugs optional.
5. **Crawl files**: dynamic `sitemap.xml` (with `xhtml:link` alternates and `lastmod`), `robots.txt` (admin disallowed, sitemap listed), clean 404/410 pages, 301 for renamed slugs.
6. **Performance (Core Web Vitals)**: responsive `<img srcset sizes>` with width/height set, `loading="lazy"` except the LCP hero (`fetchpriority="high"`), avif/webp, CDN + immutable caching, Brotli/gzip, critical CSS inlining (Angular default), minimal JS, no layout shift from fonts/images, `preconnect` only where needed. Lighthouse CI budget: Performance ≥ 95, SEO 100, LCP < 2.0 s on mobile.
7. **Content**: semantic HTML (one `h1`, ordered headings, landmarks, descriptive alt text from `MediaAssets.Alt`), crawlable `<a href>` links (not click handlers), accessible contrast and focus states.
8. **Ops**: Google Search Console + Bing verification, submit sitemap, monitor with Lighthouse CI in the pipeline.

## 10. Deployment

- **Containers**: three images — `api` (ASP.NET), `client` (Node SSR server `dist/client/server/server.mjs`), `admin` (static files on nginx) — behind a reverse proxy (nginx/Caddy/Traefik) with TLS: `example.com` → client, `admin.example.com` → admin, `example.com/api` & `/media` → API (or `api.example.com`).
- **Database**: managed SQL Server/Azure SQL; apply migrations as a deploy step (`dotnet ef migrations bundle`), not on app start in production. Nightly backups.
- **Config**: env vars per environment (`ConnectionStrings__Default`, `Jwt__Key`, `Cors__Origins__*`, storage settings). `allowedHosts` set in Angular SSR config for the real domain.
- **CI/CD (GitHub Actions)**: backend build+test; frontend build+test for both apps; Lighthouse CI against the SSR build; build/push images; deploy. Cache with CDN; purge media/content cache on admin save (or short TTL + ETag).
- **Observability**: structured logging (Serilog), health endpoint, error tracking.

## 11. Phased roadmap

| Phase | Scope | Done when |
|---|---|---|
| 0 — Foundation ✅ | Solution/workspace scaffolds, data model + migration, JWT login, SSR shell, SEO service, fonts/logo, README, tests | Everything builds, tests pass |
| 1 — Content core ✅ | Seeder from `content.json` + CV; public content/site/cv/projects endpoints; admin CRUD for texts, site settings, sections | Public hero/about/contact render from DB; texts editable |
| 2 — Media | Upload pipeline, variants, media library UI, image pickers everywhere | All site images replaceable from admin |
| 3 — Public site build-out | All sections, EN/FA + RTL, gallery + render↔sketch slider, contact form → `ContactMessages` | Visual parity with reference design |
| 4 — Projects & CV editors | Project/image editors with reorder, CV lists, messages inbox | No hard-coded content left |
| 5 — SEO | Per-route meta from admin, JSON-LD, hreflang, sitemap/robots, performance budget, Lighthouse CI | SEO 100 / Perf ≥ 95 on key pages |
| 6 — Hardening & launch | Refresh tokens, rate limits, CSP, 2FA (optional), backups, Docker/CI/CD, domain + TLS, Search Console | Production live |

## 12. Decisions and open questions

- **Domain name** (needed for canonical URLs, `allowedHosts`, sitemap) — placeholder `https://example.com` in `SeoService`.
- **Brand font**: the uploaded font file did not reach the repo; open-source stand-ins are wired. Drop the licensed woff2 into `shared-assets/fonts`.
- **Hosting target** (Azure, a VPS, etc.) decides the storage provider and deploy scripts.
- **URL scheme** for Persian: prefix (`/fa`) is recommended; confirm.
- **Contact form**: store in DB and email via SMTP/SendGrid — choose provider.
