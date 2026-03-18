import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DefaultLocationComponent } from './components/default-location/default-location.component';

@NgModule({
  declarations: [DefaultLocationComponent],
  imports: [CommonModule, FormsModule],
  exports: [DefaultLocationComponent]
})
export class SettingsModule {}