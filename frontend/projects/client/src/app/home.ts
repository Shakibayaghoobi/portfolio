import { toSignal } from '@angular/core/rxjs-interop';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';
import { ContentMap, Locale, PublicApi, SiteData, isRtl, renderMarkup, t } from 'shared';
import { SeoService } from './seo.service';

/**
 * Home page. Every string comes from the API (content + site settings); sections render in the order, and only if
 * visible, as set in admin. Sections without a template yet (works, studies, ...) are built in Phase 3.
 */
@Component({
  selector: 'pf-home',
  host: { '[attr.dir]': 'dir()', '[attr.lang]': 'locale()', '[style.--lit]': 'accent()' },
  template: `
    <header class="wrap bar">
      <a href="/" class="mono brand"><img src="/brand/logo.svg" alt="" width="28" height="28" />{{ setting('site.name') }}</a>
      <button type="button" class="lang mono" (click)="toggleLocale()" [attr.lang]="locale() === 'en' ? 'fa' : 'en'">
        {{ locale() === 'en' ? 'فارسی' : 'EN' }}
      </button>
    </header>

    <main>
      @for (section of site()?.sections ?? []; track section.key) {
        @switch (section.key) {
          @case ('hero') {
            <section class="wrap hero" id="hero">
              <span class="mono lit">{{ tx('hero.role') }}</span>
              <h1>{{ setting('site.name') }}</h1>
              <p class="tag">{{ tx('hero.tag') }}</p>
              <p class="ctas">
                <a class="btn primary" href="#works">{{ tx('hero.cta1') }}</a>
                <a class="btn" href="#contact">{{ tx('hero.cta2') }}</a>
              </p>
            </section>
          }
          @case ('about') {
            <section class="wrap about" id="about">
              <span class="mono lit">{{ tx('about.k') }}</span>
              <h2 [innerHTML]="html('about.h')"></h2>
              <p>{{ tx('about.p1') }}</p>
              <p>{{ tx('about.p2') }}</p>
            </section>
          }
          @case ('contact') {
            <section class="wrap contact" id="contact">
              <span class="mono lit">{{ tx('contact.k') }}</span>
              <h2 [innerHTML]="html('contact.h')"></h2>
              <dl>
                <dt class="mono">{{ tx('contact.email') }}</dt>
                <dd><a [href]="'mailto:' + setting('contact.email', false)">{{ setting('contact.email', false) }}</a></dd>
                <dt class="mono">{{ tx('contact.phone') }}</dt>
                <dd><a [href]="'tel:' + setting('contact.phone', false)" dir="ltr">{{ setting('contact.phoneText', false) }}</a></dd>
                <dt class="mono">{{ tx('contact.ig') }}</dt>
                <dd><a [href]="'https://instagram.com/' + setting('contact.instagram', false)" rel="noopener">&#64;{{ setting('contact.instagram', false) }}</a></dd>
                <dt class="mono">{{ tx('contact.studio') }}</dt>
                <dd>{{ setting('contact.address') }}</dd>
              </dl>
            </section>
          }
        }
      }
    </main>
  `,
  styles: `
    .bar { padding-block: 20px; display: flex; justify-content: space-between; align-items: center; }
    .brand { display: inline-flex; align-items: center; gap: 12px; }
    .brand img { filter: invert(1); }
    .lang { background: none; border: 1px solid var(--line2); color: inherit; padding: 8px 14px; border-radius: 999px; cursor: pointer; }
    section { padding-block: clamp(48px, 8vw, 120px); }
    h1 { font-size: clamp(48px, 7vw, 104px); font-weight: 300; line-height: 0.95; margin: 24px 0; }
    h2 { font-size: clamp(32px, 4vw, 56px); font-weight: 300; line-height: 1.05; margin: 16px 0 24px; }
    h2 :is(em) { font-style: normal; color: var(--lit); }
    .tag { font-size: clamp(20px, 2vw, 28px); max-width: 28ch; color: var(--mute); }
    .btn { display: inline-block; padding: 12px 22px; border: 1px solid var(--line2); border-radius: 999px; margin-inline-end: 12px; }
    .btn.primary { background: var(--lit); color: #14110e; border-color: var(--lit); }
    p { max-width: 62ch; line-height: 1.6; }
    dl { display: grid; grid-template-columns: max-content 1fr; gap: 12px 32px; }
    dd, dt { margin: 0; }
  `,
})
export class Home {
  private api = inject(PublicApi);

  readonly locale = signal<Locale>('en');
  readonly dir = computed(() => (isRtl(this.locale()) ? 'rtl' : 'ltr'));

  readonly content = toSignal(this.api.content().pipe(catchError(() => of({} as ContentMap))), { initialValue: {} as ContentMap });
  readonly site = toSignal<SiteData | null>(this.api.site().pipe(catchError(() => of(null))), { initialValue: null });
  readonly accent = computed(() => this.site()?.settings['site.accent']?.en || null);

  constructor() {
    const seo = inject(SeoService);
    effect(() => {
      const name = this.setting('site.name');
      if (!name) return;
      const role = this.tx('hero.role');
      seo.update({
        title: role ? `${name} — ${role}` : name,
        description: this.tx('about.p1').slice(0, 160),
        path: '/',
      });
    });
  }

  toggleLocale() { this.locale.update(l => (l === 'en' ? 'fa' : 'en')); }

  /** Copy for a content key in the current language. */
  tx(key: string): string { return t(this.content()[key], this.locale()); }
  /** Copy with the light markup rendered to safe HTML. */
  html(key: string): string { return renderMarkup(this.tx(key)); }
  /** Site setting; language-neutral values (email, phone) pass localized = false. */
  setting(key: string, localized = true): string {
    const v = this.site()?.settings[key];
    return v ? (localized ? t(v, this.locale()) : v.en) : '';
  }
}
