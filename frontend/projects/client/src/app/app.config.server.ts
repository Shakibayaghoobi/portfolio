import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { API_BASE_URL } from 'shared';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes)),
    // On the server there is no dev proxy, so call the API directly (set API_URL in production).
    { provide: API_BASE_URL, useValue: process.env['API_URL'] ?? 'http://localhost:5128/api' },
  ],
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
