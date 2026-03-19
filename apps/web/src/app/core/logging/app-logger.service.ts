import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ApplicationInsightsService } from '../observability/application-insights.service';

@Injectable({ providedIn: 'root' })
export class AppLoggerService {
  private readonly debugLogging = !!environment.enableDebugLogging;

  constructor(private readonly appInsights: ApplicationInsightsService) {}

  debug(source: string, message: string, context?: unknown): void {
    if (this.debugLogging) {
      console.info(`[${source}] ${message}`, context ?? '');
    }

    this.appInsights.trackTrace(`[${source}] ${message}`, this.toProperties(context));
  }

  error(source: string, message: string, error: unknown): void {
    console.error(`[${source}] ${message}`, error);
    this.appInsights.trackException(error, {
      source,
      message
    });
  }

  private toProperties(context?: unknown): Record<string, unknown> | undefined {
    if (!context || typeof context !== 'object') {
      return undefined;
    }

    return context as Record<string, unknown>;
  }
}