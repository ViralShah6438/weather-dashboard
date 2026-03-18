import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { DashboardPageComponent } from './dashboard-page.component';
import { WeatherApiService } from '../../../../core/api/weather-api.service';
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
    fixture.detectChanges();
  });

  it('loads default location and weather on init', () => {
    expect(apiSpy.getDefaultLocation).toHaveBeenCalled();
    expect(apiSpy.getWeather).toHaveBeenCalledWith('London');

    let latestWeatherCity: string | undefined;
    component.weather$.subscribe((value) => {
      latestWeatherCity = value?.city;
    }).unsubscribe();

    expect(latestWeatherCity).toBe('London');
  });

  it('shows error when weather request fails', () => {
    apiSpy.getWeather.and.returnValue(throwError(() => ({ error: { detail: 'City not found' } })));

    component.onSearch('Unknown');

    let latestErrorMessage: string | null = null;
    component.errorMessage$.subscribe((value) => {
      latestErrorMessage = value;
    }).unsubscribe();

    expect(latestErrorMessage).toContain('City not found');
  });
});
