import type { WeatherViewModel } from '../types/weather';

type WeatherCardProps = {
  weather: WeatherViewModel;
};

export function WeatherCard({ weather }: WeatherCardProps) {
  return (
    <article className="card">
      <header className="card-header">
        <div>
          <h2>{weather.city}</h2>
          <p>{weather.description}</p>
        </div>
        <img
          src={`https://openweathermap.org/img/wn/${weather.iconCode}@2x.png`}
          alt={weather.description}
          width={90}
          height={90}
        />
      </header>

      <section className="metrics">
        <div>
          <span>Temperature</span>
          <strong>{weather.temperatureC.toFixed(1)} C</strong>
        </div>
        <div>
          <span>Humidity</span>
          <strong>{weather.humidity}%</strong>
        </div>
        <div>
          <span>Wind</span>
          <strong>{weather.windSpeedKph.toFixed(1)} km/h</strong>
        </div>
      </section>
    </article>
  );
}
