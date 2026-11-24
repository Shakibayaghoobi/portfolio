export type Locale = 'en' | 'fa';

export interface LocalizedText { en: string; fa: string; }

export const LOCALES: Locale[] = ['en', 'fa'];
export const isRtl = (l: Locale) => l === 'fa';

/** Pick the text for a locale, falling back to English when a translation is empty. */
export function t(v: LocalizedText | null | undefined, locale: Locale): string {
  return (v && (v[locale] || v.en)) || '';
}

export interface MediaRef { id: number; url: string; alt: LocalizedText; }
export interface SectionInfo { key: string; sortOrder: number; }
export interface SiteData {
  settings: Record<string, LocalizedText>;
  media: Record<string, MediaRef>;
  sections: SectionInfo[];
}

export interface ProjectImage { render: MediaRef; sketch: MediaRef | null; }
export interface Project {
  slug: string; number: string;
  title: LocalizedText; type: LocalizedText; note: LocalizedText;
  images: ProjectImage[];
}

export interface Cv {
  services: { title: LocalizedText; text: LocalizedText }[];
  awards: { year: number; title: LocalizedText; by: LocalizedText }[];
  education: { from: number; to: number | null; title: LocalizedText; organization: LocalizedText }[];
  experience: { from: number; to: number | null; organization: LocalizedText; role: LocalizedText; note: LocalizedText }[];
  skills: { name: string; value: number }[];
  languages: { name: LocalizedText; level: LocalizedText }[];
}

// ---- admin ----
export interface AdminContentEntry { id: number; key: string; group: string | null; value: LocalizedText; }
export interface AdminSetting { key: string; value: LocalizedText; media: MediaRef | null; }
export interface AdminSection { key: string; sortOrder: number; isVisible: boolean; }

/**
 * Turns the light markup used in copy into safe HTML: `*word*` becomes <em>, `<br>` a line break.
 * Everything else is escaped, so it is safe to bind with [innerHTML].
 */
export function renderMarkup(value: string): string {
  const escaped = value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  return escaped.replace(/&lt;br\s*\/?&gt;/gi, '<br>').replace(/\*([^*\n]+)\*/g, '<em>$1</em>');
}
