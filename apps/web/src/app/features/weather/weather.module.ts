import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsModule } from '../settings/settings.module';
import { SharedModule } from '../../shared/shared.module';
import { SearchBarComponent } from './components/search-bar/search-bar.component';
import { WeatherDisplayComponent } from './components/weather-display/weather-display.component';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';

@NgModule({
  declarations: [DashboardPageComponent, SearchBarComponent, WeatherDisplayComponent],
  imports: [CommonModule, FormsModule, SettingsModule, SharedModule]
})
export class WeatherModule {}