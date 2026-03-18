import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DefaultLocationDto, WeatherViewModel } from '../models/weather.models';

@Injectable({ providedIn: 'root' })
export class WeatherApiService {
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  constructor(private readonly http: HttpClient) {}

  getWeather(city: string): Observable<WeatherViewModel> {
    const params = new HttpParams().set('city', city);
    return this.http.get<WeatherViewModel>(`${this.baseUrl}/weather`, { params });
  }

  getDefaultLocation(): Observable<DefaultLocationDto> {
    return this.http.get<DefaultLocationDto>(`${this.baseUrl}/settings/default-location`);
  }

  setDefaultLocation(city: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/settings/default-location`, { city });
  }
}
