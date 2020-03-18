import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  templateUrl: './recovery-code.component.html',
  styleUrls: ['./recovery-code.component.css']
})
export class RecoveryCodeComponent {

  constructor(
    public dialogRef: MatDialogRef<RecoveryCodeComponent>,
    @Inject(MAT_DIALOG_DATA) public data: string[]) { }

  selectText(ev: MouseEvent) {
    let el = ev.target as HTMLElement;
    let range = document.createRange();
    range.selectNode(el);
    window.getSelection().removeAllRanges();
    window.getSelection().addRange(range);
  }

}
