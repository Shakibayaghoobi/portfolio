import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApi, AdminSetting } from 'shared';
import { EDITOR_STYLES } from './editor.styles';

/** Settings that are the same in both languages are edited as one value. */
const SINGLE_VALUE = new Set(['site.accent', 'contact.email', 'contact.phone', 'contact.phoneText', 'contact.instagram']);

class SettingRow {
  readonly en = signal('');
  readonly fa = signal('');
  readonly savedEn = signal('');
  readonly savedFa = signal('');
  readonly status = signal<'idle' | 'saving' | 'saved' | 'error'>('idle');
  readonly error = signal('');
  readonly dirty = computed(() => this.en() !== this.savedEn() || this.fa() !== this.savedFa());
  readonly single: boolean;
  constructor(readonly key: string, en: string, fa: string) {
    this.single = SINGLE_VALUE.has(key);
    this.apply(en, fa);
  }
  apply(en: string, fa: string) {
    this.en.set(en); this.fa.set(fa); this.savedEn.set(en); this.savedFa.set(fa);
  }
}

@Component({
  selector: 'adm-site',
  imports: [FormsModule],
  template: `
    <h2>Site settings</h2>
    <p class="hint">Name, accent colour and contact details used across the public site.</p>
    @if (loadError()) { <p class="err">{{ loadError() }}</p> }
    @for (row of rows(); track row.key) {
      <div class="row" [class.single]="row.single">
        <span class="key">{{ row.key }}</span>
        <input type="text" [ngModel]="row.en()" (ngModelChange)="onEn(row, $event)" [attr.aria-label]="row.key" [attr.dir]="row.single ? 'ltr' : null" />
        @if (!row.single) {
          <input type="text" class="fa" dir="rtl" lang="fa" [ngModel]="row.fa()" (ngModelChange)="row.fa.set($event)" [attr.aria-label]="row.key + ' Persian'" />
        }
        <span>
          <button type="button" [disabled]="!row.dirty() || row.status() === 'saving'" (click)="save(row)">Save</button>
          @switch (row.status()) {
            @case ('saved') { <span class="ok"> Saved</span> }
            @case ('error') { <span class="err"> {{ row.error() || 'Failed' }}</span> }
          }
        </span>
      </div>
    }
    @if (images().length) {
      <h2 style="margin-top: 40px">Images</h2>
      <p class="hint">Site images are placeholders until the media library arrives (Phase 2).</p>
      @for (i of images(); track i.key) { <div class="row single"><span class="key">{{ i.key }}</span><span class="key">{{ i.media?.url }}</span><span></span></div> }
    }
  `,
  styles: EDITOR_STYLES,
})
export class SitePage implements OnInit {
  private api = inject(AdminApi);
  readonly rows = signal<SettingRow[]>([]);
  readonly images = signal<AdminSetting[]>([]);
  readonly loadError = signal('');

  ngOnInit() {
    this.api.settings().subscribe({
      next: items => {
        this.rows.set(items.filter(s => !s.media).map(s => new SettingRow(s.key, s.value.en, s.value.fa)));
        this.images.set(items.filter(s => s.media));
      },
      error: () => this.loadError.set('Could not load settings. Is the API running and are you signed in?'),
    });
  }

  onEn(row: SettingRow, v: string) {
    row.en.set(v);
    if (row.single) row.fa.set(v);
  }

  save(row: SettingRow) {
    row.status.set('saving'); row.error.set('');
    this.api.saveSetting(row.key, { en: row.en(), fa: row.fa() }).subscribe({
      next: saved => { row.apply(saved.value.en, saved.value.fa); row.status.set('saved'); },
      error: err => { row.error.set(err?.error?.error ?? ''); row.status.set('error'); },
    });
  }
}
