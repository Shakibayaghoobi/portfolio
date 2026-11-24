import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'adm-login',
  imports: [FormsModule],
  template: `
    <form class="card" (ngSubmit)="submit()">
      <h1>Admin sign in</h1>
      <label>Email <input name="email" type="email" [(ngModel)]="email" required autocomplete="username" /></label>
      <label>Password <input name="password" type="password" [(ngModel)]="password" required autocomplete="current-password" /></label>
      @if (error()) { <p class="err">{{ error() }}</p> }
      <button type="submit">Sign in</button>
    </form>
  `,
  styles: `
    .card { max-width: 380px; margin: 12vh auto; display: grid; gap: 16px; padding: 32px; border: 1px solid var(--line2); border-radius: 12px; }
    h1 { margin: 0; font-weight: 300; }
    label { display: grid; gap: 6px; }
    input { padding: 10px 12px; background: var(--surface); color: var(--ink); border: 1px solid var(--line2); border-radius: 8px; }
    button { padding: 12px; background: var(--lit); color: #14110e; border: 0; border-radius: 999px; cursor: pointer; }
    .err { color: #ff8a80; margin: 0; }
  `,
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);
  email = ''; password = '';
  error = signal('');
  submit() {
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.error.set('Invalid email or password.'),
    });
  }
}
