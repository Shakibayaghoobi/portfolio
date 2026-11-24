import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('Admin App', () => {
  beforeEach(() => TestBed.configureTestingModule({ imports: [App], providers: [provideRouter(routes)] }).compileComponents());
  it('creates the app', () => expect(TestBed.createComponent(App).componentInstance).toBeTruthy());
});
