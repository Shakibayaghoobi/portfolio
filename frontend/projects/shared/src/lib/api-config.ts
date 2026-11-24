import { InjectionToken } from '@angular/core';

/** Base URL of the ASP.NET Core API. Defaults to the dev proxy path "/api". */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', { providedIn: 'root', factory: () => '/api' });
