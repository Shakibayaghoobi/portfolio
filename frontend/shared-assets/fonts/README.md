# Fonts

Self-hosted, served from `/fonts/*` in both apps (see `angular.json` assets).

- **Barlow Semi Condensed** (Latin) and **Vazirmatn** (Persian/Arabic + Latin digits): open-source fallbacks (SIL OFL) from `@fontsource`.
- **Brand font**: drop the licensed font files (woff2) here and register them in
  `frontend/styles/_fonts.scss` (a commented `@font-face` block is ready). Then put its name first
  in `$font-latin` in `frontend/styles/_tokens.scss`.
