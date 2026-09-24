import { useTranslation } from 'react-i18next';
import './LanguageSwitcher.css';

type LanguageCode = 'en' | 'es';

const LANGUAGES: { code: LanguageCode; labelKey: string }[] = [
  { code: 'en', labelKey: 'language.en' },
  { code: 'es', labelKey: 'language.es' },
];

export default function LanguageSwitcher() {
  const { i18n, t } = useTranslation();
  const active: LanguageCode =
    i18n.resolvedLanguage === 'es' ? 'es' : 'en';

  const changeLanguage = (lng: LanguageCode) => {
    void i18n.changeLanguage(lng);
  };

  return (
    <div
      className="language-switcher"
      role="group"
      aria-label={t('language.switcherLabel')}
    >
      {LANGUAGES.map(({ code, labelKey }) => (
        <button
          key={code}
          type="button"
          className={
            active === code
              ? 'language-switcher__button language-switcher__button--active'
              : 'language-switcher__button'
          }
          aria-pressed={active === code}
          onClick={() => changeLanguage(code)}
        >
          {t(labelKey)}
        </button>
      ))}
    </div>
  );
}
