/** Styles shared by the editor pages. */
export const EDITOR_STYLES = `
  h2 { font-weight: 300; font-size: 36px; margin: 0 0 8px; }
  .hint { color: var(--mute); margin: 0 0 24px; max-width: 70ch; }
  .row { display: grid; grid-template-columns: 200px 1fr 1fr auto; gap: 12px; padding: 14px 0; border-top: 1px solid var(--line); align-items: start; }
  .row.single { grid-template-columns: 200px 1fr auto; }
  .key { font-family: ui-monospace, Consolas, monospace; font-size: 12px; color: var(--mute); word-break: break-all; padding-top: 10px; }
  textarea, input[type='text'], select { width: 100%; padding: 10px 12px; background: var(--surface); color: var(--ink); border: 1px solid var(--line2); border-radius: 8px; font: inherit; }
  textarea { resize: vertical; min-height: 44px; }
  .fa { direction: rtl; }
  button { padding: 10px 18px; background: var(--lit); color: #14110e; border: 0; border-radius: 999px; cursor: pointer; font: inherit; }
  button:disabled { opacity: 0.35; cursor: default; }
  button.ghost { background: none; color: var(--ink); border: 1px solid var(--line2); }
  .ok { color: #9be2a4; } .err { color: #ff8a80; }
  .bar { display: flex; gap: 12px; align-items: center; margin-bottom: 16px; }
  .cols { display: grid; grid-template-columns: 200px 1fr 1fr auto; gap: 12px; color: var(--mute); font-size: 12px; letter-spacing: .12em; text-transform: uppercase; padding-bottom: 8px; }
`;
