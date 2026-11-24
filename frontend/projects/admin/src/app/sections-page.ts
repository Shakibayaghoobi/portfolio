import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { AdminApi, AdminSection } from 'shared';
import { EDITOR_STYLES } from './editor.styles';

@Component({
  selector: 'adm-sections',
  template: `
    <h2>Page sections</h2>
    <p class="hint">Reorder sections and hide the ones you do not want on the public page. Changes apply when you press Save.</p>
    @if (message()) { <p [class]="failed() ? 'err' : 'ok'">{{ message() }}</p> }
    @for (s of items(); track s.key; let i = $index) {
      <div class="row single">
        <span class="key">{{ s.key }}</span>
        <label><input type="checkbox" [checked]="s.isVisible" (change)="setVisible(i, $any($event.target).checked)" /> Visible</label>
        <span>
          <button type="button" class="ghost" [disabled]="i === 0" (click)="move(i, -1)" [attr.aria-label]="'Move ' + s.key + ' up'">↑</button>
          <button type="button" class="ghost" [disabled]="i === items().length - 1" (click)="move(i, 1)" [attr.aria-label]="'Move ' + s.key + ' down'">↓</button>
        </span>
      </div>
    }
    <p><button type="button" [disabled]="!dirty() || saving()" (click)="save()">Save order</button></p>
  `,
  styles: EDITOR_STYLES,
})
export class SectionsPage implements OnInit {
  private api = inject(AdminApi);
  readonly items = signal<AdminSection[]>([]);
  private saved = signal('');
  readonly saving = signal(false);
  readonly message = signal('');
  readonly failed = signal(false);
  readonly dirty = computed(() => this.fingerprint(this.items()) !== this.saved());

  ngOnInit() {
    this.api.sections().subscribe({
      next: s => this.load(s),
      error: () => { this.failed.set(true); this.message.set('Could not load sections. Is the API running and are you signed in?'); },
    });
  }

  move(i: number, delta: number) {
    const list = [...this.items()];
    const j = i + delta;
    if (j < 0 || j >= list.length) return;
    [list[i], list[j]] = [list[j], list[i]];
    this.items.set(list);
  }

  setVisible(i: number, visible: boolean) {
    this.items.update(list => list.map((s, idx) => (idx === i ? { ...s, isVisible: visible } : s)));
  }

  save() {
    this.saving.set(true); this.message.set('');
    this.api.saveSections(this.items().map(s => ({ key: s.key, isVisible: s.isVisible }))).subscribe({
      next: s => { this.load(s); this.failed.set(false); this.message.set('Saved.'); this.saving.set(false); },
      error: () => { this.failed.set(true); this.message.set('Could not save.'); this.saving.set(false); },
    });
  }

  private load(s: AdminSection[]) { this.items.set(s); this.saved.set(this.fingerprint(s)); }
  private fingerprint(s: AdminSection[]) { return s.map(x => `${x.key}:${x.isVisible}`).join('|'); }
}
