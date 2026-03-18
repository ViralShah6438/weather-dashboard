import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-search-bar',
  standalone: false,
  templateUrl: './search-bar.component.html',
  styleUrl: './search-bar.component.scss'
})
export class SearchBarComponent {
  @Input() placeholder = 'Search city';
  @Input() disabled = false;
  @Output() search = new EventEmitter<string>();

  city = '';

  submit(): void {
    const value = this.city.trim();
    if (!value) {
      return;
    }

    this.search.emit(value);
  }
}
