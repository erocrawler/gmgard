import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvGameComponent } from './adv-game.component';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { FlexLayoutModule } from '@angular/flex-layout';



@NgModule({
  declarations: [AdvGameComponent],
  imports: [
    MatCardModule,
    MatButtonModule,
    MatProgressBarModule,
    FlexLayoutModule,
    CommonModule
  ],
  exports: [AdvGameComponent],
})
export class AdvGameModule { }
