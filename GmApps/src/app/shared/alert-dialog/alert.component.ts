import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';

export function alertDialog(dialog: MatDialog, data: AlertArg): Observable<boolean> {
  return dialog.open<AlertComponent, AlertArg, boolean>(AlertComponent, { data: data }).afterClosed();
}

export interface AlertArg {
  title: string
  message: string
}

@Component({
  template: `
<h1 mat-dialog-title>{{title}}</h1>
<div mat-dialog-content>{{ message }}</div>
<div mat-dialog-actions>
    <button mat-button [mat-dialog-close]="false">取消</button>
    <button mat-raised-button color="primary" [mat-dialog-close]="true">确认</button>
</div>
`
})
export class AlertComponent {

  constructor(
    public dialogRef: MatDialogRef<AlertComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) public data: AlertArg) {
    this.title = data.title
    this.message = data.message
  }

  title: string
  message: string
}
