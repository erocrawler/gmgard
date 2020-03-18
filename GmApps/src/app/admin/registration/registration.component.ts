import { Component, OnInit } from '@angular/core';
import { AdminService } from '../admin.service';
import { InvitationCodeResponse, GetInvitationCodeRequest } from '../../models/Invitations';
import { MatSnackBar, MatDialog, MatDialogConfig } from '@angular/material';
import { DeleteConfirmationComponent, ConfirmationArgs } from './delete-confirmation.component';

@Component({
    selector: 'app-registration',
    templateUrl: './registration.component.html',
    styleUrls: ['./registration.component.css']
})
export class RegistrationComponent implements OnInit {

    username: string;
    code: string;
    searchMode = "byUser";
    loading = false;
    result: InvitationCodeResponse;
    confirmation: ConfirmationArgs = { notice: false, confirm: false };

    constructor(
        private service: AdminService,
        public snackBar: MatSnackBar,
        public dialog: MatDialog) {
    }

    ngOnInit() {

    }

    search() {
        let request: GetInvitationCodeRequest = this.searchMode === "byCode"
            ? { code: this.code }
            : { userName: this.username };
        this.loading = true
        this.service.getInvitationCode(request).subscribe((resp: InvitationCodeResponse) => {
            this.loading = false;
            this.result = resp;
        }, error => {
            this.loading = false;
            let msg = "请求失败，请重试。";
            if (error.message) {
                msg = error.message;
            }
            this.snackBar.open(msg, null, { duration: 3000 })
        })
    }

    searchUser(name: string) {
        this.searchMode = "byUser";
        this.username = name;
        this.search();
    }

    confirmDelete(code: string) {
        this.confirmation.code = code;
        this.confirmation.user = this.username;
        let dref = this.dialog.open<DeleteConfirmationComponent, ConfirmationArgs, ConfirmationArgs>(DeleteConfirmationComponent, { data: this.confirmation });
        dref.afterClosed().subscribe(a => {
            if (!a || !a.confirm) {
                return;
            }
            this.confirmation = a;
            this.service.deleteInvitationCode(a.code, a.reason, a.notice).subscribe(resp => {
                this.loading = false;
                this.result.codes = this.result.codes.filter(c => c.code != a.code)
            }, error => {
                this.loading = false;
                let msg = "请求失败，请重试。";
                if (error.message) {
                    msg = error.message;
                }
                this.snackBar.open(msg, null, { duration: 3000 })
            })
        });
    }
}
