import { Component, inject } from '@angular/core';
import { SeoService } from './seo.service';

@Component({
  selector: 'pf-home',
  template: `
    <header class="wrap bar">
      <a href="/" class="mono brand"><img src="/brand/logo.svg" alt="" width="28" height="28" />Nima Adlimoghadam</a>
    </header>
    <main class="wrap">
      <span class="mono lit">Architect / Designer — Mashhad, Iran</span>
      <h1>Foundation ready</h1>
      <p>Public site scaffold. Content will be loaded from the API and editable from the admin app.</p>
    </main>
  `,
  styles: `
    .bar { padding-block: 20px; }
    .brand { display: inline-flex; align-items: center; gap: 12px; }
    .brand img { filter: invert(1); }
    h1 { font-size: clamp(48px, 7vw, 104px); font-weight: 300; line-height: 0.95; margin: 24px 0; }
  `,
})
export class Home {
  constructor() {
    inject(SeoService).update({
      title: 'Nima Adlimoghadam — Architect & Designer',
      description: 'Architect and designer in Mashhad: façades, interiors and villas.',
      path: '/',
    });
  }
}
