import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppLoggerService } from '../logging/app-logger.service';
import { DefaultLocationDto, WeatherViewModel } from '../models/weather.models';

@Injectable({ providedIn: 'root' })
export class WeatherApiService {
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');
  private readonly source = 'WeatherApiService';

  constructor(
    private readonly http: HttpClient,
    private readonly logger: AppLoggerService
  ) {}

  getWeather(city: string): Observable<WeatherViewModel> {
    const params = new HttpParams().set('city', city);
    this.logger.debug(this.source, 'getWeather request', { city, url: `${this.baseUrl}/weather` });
    return this.http.get<WeatherViewModel>(`${this.baseUrl}/weather`, { params }).pipe(
      tap({
        next: (weather) => this.logger.debug(this.source, 'getWeather success', { city: weather.city }),
        error: (error) => this.logger.error(this.source, 'getWeather failed', error)
      })
    );
  }

  getDefaultLocation(): Observable<DefaultLocationDto> {
    this.logger.debug(this.source, 'getDefaultLocation request', { url: `${this.baseUrl}/settings/default-location` });
    return this.http.get<DefaultLocationDto>(`${this.baseUrl}/settings/default-location`).pipe(
      tap({
        next: (response) => this.logger.debug(this.source, 'getDefaultLocation success', { city: response.city }),
        error: (error) => this.logger.error(this.source, 'getDefaultLocation failed', error)
      })
    );
  }

  setDefaultLocation(city: string): Observable<void> {
    this.logger.debug(this.source, 'setDefaultLocation request', { city, url: `${this.baseUrl}/settings/default-location` });
    return this.http.put<void>(`${this.baseUrl}/settings/default-location`, { city }).pipe(
      tap({
        next: () => this.logger.debug(this.source, 'setDefaultLocation success', { city }),
        error: (error) => this.logger.error(this.source, 'setDefaultLocation failed', error)
      })
    );
  }
}
