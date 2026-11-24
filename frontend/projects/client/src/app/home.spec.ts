import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Home } from './home';

describe('Home', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const fixture = TestBed.createComponent(Home);
    const http = TestBed.inject(HttpTestingController);
    return { fixture, http, el: fixture.nativeElement as HTMLElement };
  }

  it('renders copy from the API in the order set by the sections list, skipping hidden ones', async () => {
    const { fixture, http, el } = setup();
    http.expectOne('/api/public/content').flush({
      'hero.role': { en: 'Architect', fa: 'معمار' },
      'hero.tag': { en: 'Tagline', fa: 'شعار' },
      'about.k': { en: 'Approach', fa: 'رویکرد' },
      'about.h': { en: 'Read *twice*', fa: 'دو بار' },
    });
    http.expectOne('/api/public/site').flush({
      settings: { 'site.name': { en: 'Nima', fa: 'نیما' }, 'site.accent': { en: '#112233', fa: '#112233' } },
      media: {},
      sections: [{ key: 'about', sortOrder: 0 }, { key: 'hero', sortOrder: 1 }],
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const sections = Array.from(el.querySelectorAll('section')).map(s => s.id);
    expect(sections).toEqual(['about', 'hero']);
    expect(el.querySelector('h1')?.textContent).toContain('Nima');
    expect(el.querySelector('#about h2')?.innerHTML).toContain('<em>twice</em>');
    expect(el.querySelector('#contact')).toBeNull();
  });

  it('switches to Persian and right-to-left', async () => {
    const { fixture, http, el } = setup();
    http.expectOne('/api/public/content').flush({ 'hero.tag': { en: 'Tagline', fa: 'شعار' } });
    http.expectOne('/api/public/site').flush({ settings: {}, media: {}, sections: [{ key: 'hero', sortOrder: 0 }] });
    await fixture.whenStable();
    fixture.componentInstance.toggleLocale();
    fixture.detectChanges();
    expect(el.getAttribute('dir')).toBe('rtl');
    expect(el.querySelector('.tag')?.textContent).toContain('شعار');
  });

  it('survives an API failure with an empty page', async () => {
    const { fixture, http, el } = setup();
    http.expectOne('/api/public/content').error(new ProgressEvent('error'));
    http.expectOne('/api/public/site').error(new ProgressEvent('error'));
    await fixture.whenStable();
    fixture.detectChanges();
    expect(el.querySelectorAll('section').length).toBe(0);
  });
});
