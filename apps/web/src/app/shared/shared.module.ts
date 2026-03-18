import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { LoadingStateComponent } from './ui/loading-state/loading-state.component';

@NgModule({
  declarations: [LoadingStateComponent],
  imports: [CommonModule],
  exports: [LoadingStateComponent]
})
export class SharedModule {}