import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface ConfirmationArgs {
    code?: string
    user?: string
    reason?: string
    notice: boolean
    confirm: boolean
}

@Component({
  selector: 'app-delete-confirmation',
  templateUrl: './delete-confirmation.component.html',
  styleUrls: ['./delete-confirmation.component.css']
})
export class DeleteConfirmationComponent implements OnInit {

    constructor(
        public dialogRef: MatDialogRef<DeleteConfirmationComponent, ConfirmationArgs>,
        @Inject(MAT_DIALOG_DATA) public data: ConfirmationArgs) { }

    ngOnInit() {
    }

    close(confirm: boolean) {
        this.data.confirm = confirm;
        this.dialogRef.close(this.data);
    }

}
