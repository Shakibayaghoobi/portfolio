import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApi, AdminContentEntry } from 'shared';
import { EDITOR_STYLES } from './editor.styles';

/** One editable text: current (edited) values plus the last saved values, so unsaved changes are visible. */
class TextRow {
  readonly en = signal('');
  readonly fa = signal('');
  readonly savedEn = signal('');
  readonly savedFa = signal('');
  readonly status = signal<'idle' | 'saving' | 'saved' | 'error'>('idle');
  readonly dirty = computed(() => this.en() !== this.savedEn() || this.fa() !== this.savedFa());
  constructor(readonly key: string, readonly group: string, en: string, fa: string) { this.apply(en, fa); }
  apply(en: string, fa: string) {
    this.en.set(en); this.fa.set(fa); this.savedEn.set(en); this.savedFa.set(fa);
  }
}

@Component({
  selector: 'adm-texts',
  imports: [FormsModule],
  template: `
    <h2>Texts</h2>
    <p class="hint">Every piece of copy on the public site. Edit English and Persian side by side. Use *word* for emphasis and &lt;br&gt; for a line break.</p>
    <div class="bar">
      <select [ngModel]="group()" (ngModelChange)="group.set($event)" aria-label="Group" style="max-width: 240px">
        <option value="">All groups</option>
        @for (g of groups(); track g) { <option [value]="g">{{ g }}</option> }
      </select>
      @if (loadError()) { <span class="err">{{ loadError() }}</span> }
    </div>
    <div class="cols"><span>Key</span><span>English</span><span>فارسی</span><span></span></div>
    @for (row of visible(); track row.key) {
      <div class="row">
        <span class="key">{{ row.key }}</span>
        <textarea rows="2" [ngModel]="row.en()" (ngModelChange)="row.en.set($event)" [attr.aria-label]="row.key + ' English'"></textarea>
        <textarea rows="2" class="fa" dir="rtl" lang="fa" [ngModel]="row.fa()" (ngModelChange)="row.fa.set($event)" [attr.aria-label]="row.key + ' Persian'"></textarea>
        <span>
          <button type="button" [disabled]="!row.dirty() || row.status() === 'saving'" (click)="save(row)">Save</button>
          @switch (row.status()) {
            @case ('saved') { <span class="ok"> Saved</span> }
            @case ('error') { <span class="err"> Failed</span> }
          }
        </span>
      </div>
    }
  `,
  styles: EDITOR_STYLES,
})
export class TextsPage implements OnInit {
  private api = inject(AdminApi);
  readonly rows = signal<TextRow[]>([]);
  readonly group = signal('');
  readonly loadError = signal('');
  readonly groups = computed(() => [...new Set(this.rows().map(r => r.group))]);
  readonly visible = computed(() => this.rows().filter(r => !this.group() || r.group === this.group()));

  ngOnInit() {
    this.api.content().subscribe({
      next: items => this.rows.set(items.map((e: AdminContentEntry) => new TextRow(e.key, e.group ?? 'site', e.value.en, e.value.fa))),
      error: () => this.loadError.set('Could not load texts. Is the API running and are you signed in?'),
    });
  }

  save(row: TextRow) {
    row.status.set('saving');
    this.api.saveContent(row.key, { en: row.en(), fa: row.fa() }).subscribe({
      next: saved => { row.apply(saved.value.en, saved.value.fa); row.status.set('saved'); },
      error: () => row.status.set('error'),
    });
  }
}
