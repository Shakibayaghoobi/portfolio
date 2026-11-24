import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { Dashboard } from './dashboard';
import { Login } from './login';
import { SectionsPage } from './sections-page';
import { SitePage } from './site-page';
import { TextsPage } from './texts-page';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: Dashboard,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'texts' },
      { path: 'texts', component: TextsPage },
      { path: 'site', component: SitePage },
      { path: 'sections', component: SectionsPage },
    ],
  },
  { path: '**', redirectTo: '' },
];
