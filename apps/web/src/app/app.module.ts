import { provideHttpClient } from '@angular/common/http';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ApplicationInsightsService } from './core/observability/application-insights.service';
import { WeatherModule } from './features/weather/weather.module';

function initializeApplicationInsights(appInsights: ApplicationInsightsService): () => void {
  return () => appInsights.init();
}

@NgModule({
  declarations: [AppComponent],
  imports: [BrowserModule, AppRoutingModule, WeatherModule],
  providers: [
    provideHttpClient(),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApplicationInsights,
      deps: [ApplicationInsightsService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}