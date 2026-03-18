import { DefaultLocationForm } from './components/DefaultLocationForm';
import { SearchBar } from './components/SearchBar';
import { WeatherCard } from './components/WeatherCard';
import { useWeatherDashboard } from './hooks/useWeatherDashboard';

export default function App() {
  const {
    weather,
    defaultLocation,
    isLoading,
    errorMessage,
    statusMessage,
    search,
    saveDefaultLocation
  } = useWeatherDashboard();

  return (
    <section className="shell">
      <header className="hero">
        <h1>Weather Dashboard (React)</h1>
        <p>Search current weather and maintain your default location.</p>
      </header>

      <div className="layout">
        <section className="main-panel">
          <SearchBar disabled={isLoading} onSearch={search} />

          {isLoading ? <div className="loading">Loading weather data...</div> : null}
          {statusMessage ? <p className="status">{statusMessage}</p> : null}
          {errorMessage ? <p className="error">{errorMessage}</p> : null}
          {weather ? <WeatherCard weather={weather} /> : null}
        </section>

        <aside className="side-panel">
          <DefaultLocationForm city={defaultLocation} disabled={isLoading} onSave={saveDefaultLocation} />
        </aside>
      </div>
    </section>
  );
}
