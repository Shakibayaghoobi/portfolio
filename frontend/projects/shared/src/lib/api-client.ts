import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_BASE_URL } from './api-config';
import { AdminContentEntry, AdminSection, AdminSetting, Cv, LocalizedText, Project, SiteData } from './models';

export type ContentMap = Record<string, LocalizedText>;

@Injectable({ providedIn: 'root' })
export class PublicApi {
  private http = inject(HttpClient);
  private base = inject(API_BASE_URL);

  content() { return this.http.get<ContentMap>(`${this.base}/public/content`); }
  site() { return this.http.get<SiteData>(`${this.base}/public/site`); }
  projects() { return this.http.get<Project[]>(`${this.base}/public/projects`); }
  project(slug: string) { return this.http.get<Project>(`${this.base}/public/projects/${encodeURIComponent(slug)}`); }
  cv() { return this.http.get<Cv>(`${this.base}/public/cv`); }
  health() { return this.http.get<{ status: string }>(`${this.base}/health`); }
}

/** Admin endpoints (JWT is added by the admin app's interceptor). */
@Injectable({ providedIn: 'root' })
export class AdminApi {
  private http = inject(HttpClient);
  private base = inject(API_BASE_URL);

  content(group?: string) {
    return this.http.get<AdminContentEntry[]>(`${this.base}/admin/content`, { params: group ? { group } : {} });
  }
  saveContent(key: string, value: LocalizedText) {
    return this.http.put<AdminContentEntry>(`${this.base}/admin/content/${encodeURIComponent(key)}`, value);
  }
  settings() { return this.http.get<AdminSetting[]>(`${this.base}/admin/site`); }
  saveSetting(key: string, value: LocalizedText) {
    return this.http.put<AdminSetting>(`${this.base}/admin/site/${encodeURIComponent(key)}`, value);
  }
  sections() { return this.http.get<AdminSection[]>(`${this.base}/admin/sections`); }
  saveSections(items: { key: string; isVisible: boolean }[]) {
    return this.http.put<AdminSection[]>(`${this.base}/admin/sections`, items);
  }
}
