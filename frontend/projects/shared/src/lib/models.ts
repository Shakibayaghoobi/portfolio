export type Locale = 'en' | 'fa';

export interface LocalizedText { en: string; fa: string; }

export const LOCALES: Locale[] = ['en', 'fa'];
export const isRtl = (l: Locale) => l === 'fa';

/** Pick the text for a locale, falling back to English when a translation is empty. */
export function t(v: LocalizedText | null | undefined, locale: Locale): string {
  return (v && (v[locale] || v.en)) || '';
}
