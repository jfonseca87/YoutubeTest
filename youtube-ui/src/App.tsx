import { useTranslation } from 'react-i18next';
import LanguageSwitcher from './components/LanguageSwitcher';
import VideoGrid from './components/VideoGrid';
import './App.css';

function App() {
  const { t } = useTranslation();

  return (
    <div className="app">
      <header className="app__header">
        <div className="app__logo" role="img" aria-label={t('app.logoAria')}>
          <span className="app__logo-mark" aria-hidden="true" />
          <span className="app__logo-word">{t('app.title')}</span>
        </div>
        <LanguageSwitcher />
      </header>
      <main className="app__main">
        <VideoGrid />
      </main>
    </div>
  );
}

export default App;
