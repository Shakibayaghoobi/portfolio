import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './auth.service';

/** Admin shell: left navigation plus the editor for the selected area. */
@Component({
  selector: 'adm-dashboard',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell">
      <nav>
        <h1 class="mono">Admin</h1>
        <a routerLink="/texts" routerLinkActive="on">Texts</a>
        <a routerLink="/site" routerLinkActive="on">Site settings</a>
        <a routerLink="/sections" routerLinkActive="on">Page sections</a>
        <button type="button" class="out" (click)="logout()">Sign out</button>
      </nav>
      <main><router-outlet /></main>
    </div>
  `,
  styles: `
    .shell { display: grid; grid-template-columns: 220px 1fr; min-height: 100vh; }
    nav { display: flex; flex-direction: column; gap: 4px; padding: 24px 16px; border-inline-end: 1px solid var(--line); }
    nav h1 { margin: 0 0 16px 8px; font-weight: 500; }
    nav a { padding: 10px 12px; border-radius: 8px; }
    nav a.on { background: var(--surface); color: var(--lit); }
    .out { margin-top: auto; padding: 10px; background: none; color: var(--mute); border: 1px solid var(--line2); border-radius: 8px; cursor: pointer; }
    main { padding: 32px clamp(16px, 3vw, 48px); min-width: 0; }
  `,
})
export class Dashboard {
  private auth = inject(AuthService);
  private router = inject(Router);
  logout() { this.auth.logout(); this.router.navigateByUrl('/login'); }
}
