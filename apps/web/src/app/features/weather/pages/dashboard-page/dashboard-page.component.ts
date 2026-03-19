import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { BehaviorSubject, Subject, finalize, takeUntil } from 'rxjs';
import { WeatherApiService } from '../../../../core/api/weather-api.service';
import { AppLoggerService } from '../../../../core/logging/app-logger.service';
import { ApiProblemDetails, WeatherViewModel } from '../../../../core/models/weather.models';

@Component({
  selector: 'app-dashboard-page',
  standalone: false,
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly source = 'DashboardPageComponent';
  private readonly weatherSubject = new BehaviorSubject<WeatherViewModel | null>(null);
  private readonly errorMessageSubject = new BehaviorSubject<string | null>(null);
  private readonly statusMessageSubject = new BehaviorSubject<string | null>(null);
  private readonly defaultLocationSubject = new BehaviorSubject<string>('London');
  private readonly isLoadingSubject = new BehaviorSubject<boolean>(false);

  readonly weather$ = this.weatherSubject.asObservable();
  readonly errorMessage$ = this.errorMessageSubject.asObservable();
  readonly statusMessage$ = this.statusMessageSubject.asObservable();
  readonly defaultLocation$ = this.defaultLocationSubject.asObservable();
  readonly isLoading$ = this.isLoadingSubject.asObservable();

  constructor(
    private readonly weatherApi: WeatherApiService,
    private readonly logger: AppLoggerService
  ) {}

  ngOnInit(): void {
    this.logger.debug(this.source, 'Dashboard initialized');
    this.loadDefaultLocation();
  }

  ngOnDestroy(): void {
    this.logger.debug(this.source, 'Dashboard destroyed');
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(city: string): void {
    this.logger.debug(this.source, 'Manual weather search triggered', { city });
    this.searchWeather(city);
  }

  onSaveDefaultLocation(city: string): void {
    this.logger.debug(this.source, 'Saving default location', { city });
    this.isLoadingSubject.next(true);
    this.statusMessageSubject.next(null);

    this.weatherApi
      .setDefaultLocation(city)
      .pipe(
        finalize(() => this.isLoadingSubject.next(false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.defaultLocationSubject.next(city);
          this.statusMessageSubject.next(`Default location updated to ${city}.`);
          this.logger.debug(this.source, 'Default location updated', { city });
          this.searchWeather(city);
        },
        error: (error: HttpErrorResponse) => {
          this.logger.error(this.source, 'Failed to update default location', error);
          this.errorMessageSubject.next(this.resolveError(error, 'Unable to update default location.'));
        }
      });
  }

  private loadDefaultLocation(): void {
    this.logger.debug(this.source, 'Loading default location');
    this.isLoadingSubject.next(true);
    this.weatherApi
      .getDefaultLocation()
      .pipe(
        finalize(() => this.isLoadingSubject.next(false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: ({ city }) => {
          const value = city?.trim() || 'London';
          this.defaultLocationSubject.next(value);
          this.logger.debug(this.source, 'Default location loaded', { city: value });
          this.searchWeather(value);
        },
        error: (error: HttpErrorResponse) => {
          this.logger.error(this.source, 'Failed to load default location', error);
          this.errorMessageSubject.next(this.resolveError(error, 'Unable to load default location.'));
        }
      });
  }

  private searchWeather(city: string): void {
    this.logger.debug(this.source, 'Searching weather', { city });
    this.errorMessageSubject.next(null);
    this.statusMessageSubject.next(null);
    this.isLoadingSubject.next(true);

    this.weatherApi
      .getWeather(city)
      .pipe(
        finalize(() => this.isLoadingSubject.next(false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (weather) => {
          this.weatherSubject.next(weather);
          this.logger.debug(this.source, 'Weather loaded', { city: weather.city });
        },
        error: (error: HttpErrorResponse) => {
          this.weatherSubject.next(null);
          this.logger.error(this.source, 'Failed to fetch weather', error);
          this.errorMessageSubject.next(this.resolveError(error, 'Unable to fetch weather data.'));
        }
      });
  }

  private resolveError(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 0) {
      return 'Cannot reach weather API. Ensure backend is running and apiBaseUrl is set correctly for this environment.';
    }

    const details = error.error as ApiProblemDetails | undefined;
    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    return details?.detail || details?.title || error.message || fallback;
  }
}
