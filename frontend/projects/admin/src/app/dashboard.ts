import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'adm-dashboard',
  template: `
    <main class="wrap">
      <h1>Admin</h1>
      <p>Signed in. Content editors (texts, sections, projects, media, SEO) are built in the next phases.</p>
      <button (click)="logout()">Sign out</button>
    </main>
  `,
  styles: `h1 { font-weight: 300; font-size: 48px; }`,
})
export class Dashboard {
  private auth = inject(AuthService);
  private router = inject(Router);
  logout() { this.auth.logout(); this.router.navigateByUrl('/login'); }
}
