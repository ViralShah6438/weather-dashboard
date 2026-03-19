import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WeatherApiService } from './weather-api.service';

describe('WeatherApiService', () => {
  let service: WeatherApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WeatherApiService]
    });

    service = TestBed.inject(WeatherApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('requests weather with city query parameter', () => {
    service.getWeather('Berlin').subscribe((response) => {
      expect(response.city).toBe('Berlin');
    });

    const req = httpMock.expectOne((request) => {
      return request.url.endsWith('/weather') && request.params.get('city') === 'Berlin';
    });

    expect(req.request.method).toBe('GET');
    req.flush({
      city: 'Berlin',
      description: 'cloudy',
      humidity: 72,
      iconCode: '03d',
      temperatureC: 12.3,
      windSpeedKph: 8.4
    });
  });

  it('requests default location from settings endpoint', () => {
    service.getDefaultLocation().subscribe((response) => {
      expect(response.city).toBe('Madrid');
    });

    const req = httpMock.expectOne((request) => request.url.endsWith('/settings/default-location'));

    expect(req.request.method).toBe('GET');
    req.flush({ city: 'Madrid' });
  });

  it('sends city payload when saving default location', () => {
    service.setDefaultLocation('Rome').subscribe();

    const req = httpMock.expectOne((request) => request.url.endsWith('/settings/default-location'));

    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ city: 'Rome' });
    req.flush(null);
  });

  it('avoids accidental double slash before endpoint path', () => {
    service.getWeather('Oslo').subscribe();

    const req = httpMock.expectOne((request) => request.url.endsWith('/weather'));

    expect(req.request.url).not.toMatch(/\/\/weather$/);
    req.flush({
      city: 'Oslo',
      description: 'sunny',
      humidity: 40,
      iconCode: '01d',
      temperatureC: 16.7,
      windSpeedKph: 11.9
    });
  });
});
