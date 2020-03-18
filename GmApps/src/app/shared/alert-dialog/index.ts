import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule, MatDialogModule } from "@angular/material";
import { AlertComponent } from "./alert.component";

@NgModule({
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
  ],
  exports: [
    AlertComponent,
  ],
  entryComponents: [AlertComponent],
  declarations: [AlertComponent],
})
export class AlertModule { }

export { AlertComponent, AlertArg } from "./alert.component";
