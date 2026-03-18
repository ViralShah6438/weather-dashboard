import { Component } from '@angular/core';

@Component({
  selector: 'app-loading-state',
  standalone: false,
  template: '<div class="loading">Loading weather data...</div>',
  styles: [
    '.loading { background: #f0f7ff; color: #355d84; border-radius: 10px; padding: 0.7rem 0.8rem; }'
  ]
})
export class LoadingStateComponent {}
