import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { API_BASE_URL } from 'shared';
import { tap } from 'rxjs';

const KEY = 'pf_admin_token';

/** Holds the admin JWT. Replace sessionStorage with an httpOnly cookie flow if desired (see guide). */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private base = inject(API_BASE_URL);
  readonly token = signal<string | null>(typeof sessionStorage !== 'undefined' ? sessionStorage.getItem(KEY) : null);

  login(email: string, password: string) {
    return this.http.post<{ token: string }>(`${this.base}/admin/auth/login`, { email, password })
      .pipe(tap(r => { sessionStorage.setItem(KEY, r.token); this.token.set(r.token); }));
  }
  logout() { sessionStorage.removeItem(KEY); this.token.set(null); }
}
