import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_BASE_URL } from './api-config';
import { LocalizedText } from './models';

export type ContentMap = Record<string, LocalizedText>;

@Injectable({ providedIn: 'root' })
export class PublicApi {
  private http = inject(HttpClient);
  private base = inject(API_BASE_URL);

  content() { return this.http.get<ContentMap>(`${this.base}/public/content`); }
  health() { return this.http.get<{ status: string }>(`${this.base}/health`); }
}
