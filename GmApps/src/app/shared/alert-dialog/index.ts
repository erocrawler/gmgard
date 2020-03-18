import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule } from "@angular/material/dialog";
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
