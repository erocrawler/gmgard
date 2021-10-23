import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditExamToolbarComponent } from './audit-exam-toolbar.component';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MessageToolbarComponent } from './message-toolbar.component';



@NgModule({
  declarations: [
    AuditExamToolbarComponent,
    MessageToolbarComponent,
  ],
  imports: [
    CommonModule,
    MatButtonModule,
    RouterModule,
    MatIconModule,
  ],
  entryComponents: [
    AuditExamToolbarComponent,
    MessageToolbarComponent,
  ]
})
export class ToolbarModule { }
