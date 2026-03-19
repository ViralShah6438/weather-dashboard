import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, of, throwError } from 'rxjs';
import { DashboardPageComponent } from './dashboard-page.component';
import { WeatherApiService } from '../../../../core/api/weather-api.service';
import { WeatherViewModel } from '../../../../core/models/weather.models';
import { DefaultLocationComponent } from '../../../settings/components/default-location/default-location.component';
import { SearchBarComponent } from '../../components/search-bar/search-bar.component';
import { WeatherDisplayComponent } from '../../components/weather-display/weather-display.component';
import { LoadingStateComponent } from '../../../../shared/ui/loading-state/loading-state.component';

describe('DashboardPageComponent', () => {
  let fixture: ComponentFixture<DashboardPageComponent>;
  let component: DashboardPageComponent;
  let apiSpy: jasmine.SpyObj<WeatherApiService>;

  beforeEach(async () => {
    apiSpy = jasmine.createSpyObj<WeatherApiService>('WeatherApiService', [
      'getDefaultLocation',
      'setDefaultLocation',
      'getWeather'
    ]);

    apiSpy.getDefaultLocation.and.returnValue(of({ city: 'London' }));
    apiSpy.getWeather.and.returnValue(
      of({
        city: 'London',
        description: 'clear sky',
        humidity: 55,
        iconCode: '01d',
        temperatureC: 17.2,
        windSpeedKph: 10.2
      })
    );
    apiSpy.setDefaultLocation.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      declarations: [
        DashboardPageComponent,
        SearchBarComponent,
        WeatherDisplayComponent,
        DefaultLocationComponent,
        LoadingStateComponent
      ],
      imports: [CommonModule, FormsModule],
      providers: [{ provide: WeatherApiService, useValue: apiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardPageComponent);
    component = fixture.componentInstance;
  });

  function createWeather(city: string): WeatherViewModel {
    return {
      city,
      description: 'clear sky',
      humidity: 55,
      iconCode: '01d',
      temperatureC: 17.2,
      windSpeedKph: 10.2
    };
  }

  function latestValue<T>(source$: Observable<T>): T | undefined {
    let latest: T | undefined;
    const sub = source$.subscribe((value) => {
      latest = value;
    });
    sub.unsubscribe();
    return latest;
  }

  it('loads default location and weather on init', () => {
    fixture.detectChanges();

    expect(apiSpy.getDefaultLocation).toHaveBeenCalled();
    expect(apiSpy.getWeather).toHaveBeenCalledWith('London');

    const latestWeatherCity = latestValue(component.weather$)?.city;

    expect(latestWeatherCity).toBe('London');
  });

  it('falls back to London when default location is blank', () => {
    apiSpy.getDefaultLocation.and.returnValue(of({ city: '   ' }));

    fixture.detectChanges();

    expect(apiSpy.getWeather).toHaveBeenCalledWith('London');
    expect(latestValue(component.defaultLocation$)).toBe('London');
  });

  it('shows error when loading default location fails', () => {
    apiSpy.getDefaultLocation.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: { detail: 'Settings API unavailable' } }))
    );

    fixture.detectChanges();

    expect(latestValue(component.errorMessage$)).toContain('Settings API unavailable');
    expect(apiSpy.getWeather).not.toHaveBeenCalled();
  });

  it('calls weather API when search is requested', () => {
    fixture.detectChanges();
    apiSpy.getWeather.calls.reset();

    component.onSearch('Paris');

    expect(apiSpy.getWeather).toHaveBeenCalledWith('Paris');
  });

  it('updates default location and triggers weather refresh when saving default location succeeds', () => {
    fixture.detectChanges();
    apiSpy.getWeather.calls.reset();

    component.onSaveDefaultLocation('Rome');

    expect(apiSpy.setDefaultLocation).toHaveBeenCalledWith('Rome');
    expect(apiSpy.getWeather).toHaveBeenCalledWith('Rome');
    expect(latestValue(component.defaultLocation$)).toBe('Rome');
  });

  it('shows error when saving default location fails', () => {
    fixture.detectChanges();
    apiSpy.setDefaultLocation.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 400, error: { detail: 'Invalid city' } }))
    );

    component.onSaveDefaultLocation('@@');

    expect(latestValue(component.errorMessage$)).toContain('Invalid city');
  });

  it('clears weather and shows error when weather request fails', () => {
    fixture.detectChanges();
    apiSpy.getWeather.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 404, error: { detail: 'City not found' } }))
    );

    component.onSearch('Unknown');

    expect(latestValue(component.weather$)).toBeNull();
    expect(latestValue(component.errorMessage$)).toContain('City not found');
  });

  it('sets loading while weather request is in progress and clears it after completion', () => {
    fixture.detectChanges();
    const weatherResponse$ = new Subject<WeatherViewModel>();
    apiSpy.getWeather.and.returnValue(weatherResponse$.asObservable());

    component.onSearch('Berlin');
    expect(latestValue(component.isLoading$)).toBeTrue();

    weatherResponse$.next(createWeather('Berlin'));
    weatherResponse$.complete();

    expect(latestValue(component.isLoading$)).toBeFalse();
  });

  it('returns connectivity message for status 0 errors', () => {
    const message = (component as any).resolveError(
      new HttpErrorResponse({ status: 0, error: new ProgressEvent('error') }),
      'fallback'
    );

    expect(message).toContain('Cannot reach weather API.');
  });

  it('prefers string error payload over fallback message', () => {
    const message = (component as any).resolveError(
      new HttpErrorResponse({ status: 400, error: 'Invalid request payload' }),
      'fallback'
    );

    expect(message).toBe('Invalid request payload');
  });
});
