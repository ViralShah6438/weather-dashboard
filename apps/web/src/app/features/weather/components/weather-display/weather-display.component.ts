import { Component, Input } from '@angular/core';
import { WeatherViewModel } from '../../../../core/models/weather.models';

@Component({
  selector: 'app-weather-display',
  standalone: false,
  templateUrl: './weather-display.component.html',
  styleUrl: './weather-display.component.scss'
})
export class WeatherDisplayComponent {
  @Input({ required: true }) weather!: WeatherViewModel;
}
