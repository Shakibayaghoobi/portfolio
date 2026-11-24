import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

export interface SeoData { title: string; description: string; path: string; image?: string; locale?: string; }

/** Sets title, description, canonical, Open Graph and Twitter tags. Works during SSR/prerender. */
@Injectable({ providedIn: 'root' })
export class SeoService {
  private title = inject(Title);
  private meta = inject(Meta);
  private doc = inject(DOCUMENT);
  readonly siteUrl = 'https://example.com'; // TODO: move to environment/site settings

  update(d: SeoData) {
    const url = this.siteUrl + d.path;
    this.title.setTitle(d.title);
    this.meta.updateTag({ name: 'description', content: d.description });
    this.meta.updateTag({ property: 'og:title', content: d.title });
    this.meta.updateTag({ property: 'og:description', content: d.description });
    this.meta.updateTag({ property: 'og:url', content: url });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ name: 'twitter:card', content: 'summary_large_image' });
    if (d.image) this.meta.updateTag({ property: 'og:image', content: d.image });
    let link = this.doc.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    if (!link) { link = this.doc.createElement('link'); link.rel = 'canonical'; this.doc.head.appendChild(link); }
    link.href = url;
  }
}
