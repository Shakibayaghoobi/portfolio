import { RenderMode, ServerRoute } from '@angular/ssr';

// Pages are filled from the API, so render them per request instead of freezing content at build time.
export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    renderMode: RenderMode.Server,
  },
];
