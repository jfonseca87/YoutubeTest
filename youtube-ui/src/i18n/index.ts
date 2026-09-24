import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './en';
import es from './es';

export const LANGUAGE_STORAGE_KEY = 'youtube-ui-lang';

function detectLanguage(): 'en' | 'es' {
  const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY);
  if (stored === 'en' || stored === 'es') {
    return stored;
  }
  return navigator.language.toLowerCase().startsWith('es') ? 'es' : 'en';
}

i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    es: { translation: es },
  },
  lng: detectLanguage(),
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
});

i18n.on('languageChanged', (lng) => {
  localStorage.setItem(LANGUAGE_STORAGE_KEY, lng);
  document.documentElement.lang = lng;
});

document.documentElement.lang = i18n.language;

export default i18n;
