import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { SectionsPage } from './sections-page';

describe('SectionsPage', () => {
  it('moves and hides sections, then saves the new order', async () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const fixture = TestBed.createComponent(SectionsPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    const initial = [
      { key: 'hero', sortOrder: 0, isVisible: true },
      { key: 'about', sortOrder: 1, isVisible: true },
      { key: 'contact', sortOrder: 2, isVisible: true },
    ];
    http.expectOne('/api/admin/sections').flush(initial);
    await fixture.whenStable();

    const page = fixture.componentInstance;
    expect(page.dirty()).toBe(false);
    page.move(0, 1);
    page.setVisible(2, false);
    expect(page.items().map(s => s.key)).toEqual(['about', 'hero', 'contact']);
    expect(page.dirty()).toBe(true);

    page.save();
    const req = http.expectOne('/api/admin/sections');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual([
      { key: 'about', isVisible: true }, { key: 'hero', isVisible: true }, { key: 'contact', isVisible: false },
    ]);
    req.flush([
      { key: 'about', sortOrder: 0, isVisible: true }, { key: 'hero', sortOrder: 1, isVisible: true }, { key: 'contact', sortOrder: 2, isVisible: false },
    ]);
    expect(page.dirty()).toBe(false);
    expect(page.message()).toBe('Saved.');
  });
});
