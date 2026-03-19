import { Injectable } from '@angular/core';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApplicationInsightsService {
  private readonly instrumentationKey = (environment.appInsightsInstrumentationKey ?? '').trim();
  private appInsights: ApplicationInsights | null = null;

  init(): void {
    if (!this.instrumentationKey || this.appInsights) {
      return;
    }

    this.appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: this.instrumentationKey,
        enableAutoRouteTracking: true,
        disableFetchTracking: false,
        disableAjaxTracking: false
      }
    });

    this.appInsights.loadAppInsights();
    this.appInsights.trackTrace({ message: 'Application Insights initialized' });
  }

  trackTrace(message: string, properties?: Record<string, unknown>): void {
    this.appInsights?.trackTrace({ message, properties });
  }

  trackException(error: unknown, properties?: Record<string, unknown>): void {
    this.appInsights?.trackException({ exception: this.toError(error), properties });
  }

  private toError(value: unknown): Error {
    if (value instanceof Error) {
      return value;
    }

    const message = typeof value === 'string' ? value : JSON.stringify(value ?? 'Unknown error');
    return new Error(message);
  }
}