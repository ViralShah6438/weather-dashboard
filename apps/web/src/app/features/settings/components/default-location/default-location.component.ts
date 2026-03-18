import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-default-location',
  standalone: false,
  templateUrl: './default-location.component.html',
  styleUrl: './default-location.component.scss'
})
export class DefaultLocationComponent implements OnChanges {
  @Input() city = '';
  @Input() disabled = false;
  @Output() save = new EventEmitter<string>();

  draftCity = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['city']) {
      this.draftCity = this.city;
    }
  }

  submit(): void {
    const value = this.draftCity.trim();
    if (!value) {
      return;
    }

    this.save.emit(value);
  }
}
