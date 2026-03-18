import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { WeatherDisplayComponent } from './weather-display.component';

describe('WeatherDisplayComponent', () => {
  let fixture: ComponentFixture<WeatherDisplayComponent>;
  let component: WeatherDisplayComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WeatherDisplayComponent],
      imports: [CommonModule]
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherDisplayComponent);
    component = fixture.componentInstance;
    component.weather = {
      city: 'London',
      description: 'clear sky',
      humidity: 65,
      iconCode: '01d',
      temperatureC: 18.5,
      windSpeedKph: 11.2
    };
    fixture.detectChanges();
  });

  it('renders weather city and metrics', () => {
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('London');
    expect(text).toContain('Humidity');
    expect(text).toContain('Wind');
  });
});
