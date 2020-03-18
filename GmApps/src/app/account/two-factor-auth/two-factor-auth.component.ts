import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { ActivatedRoute } from '@angular/router';
import { TwoFactorAuthenticationModel } from '../../models/TwoFactorAuth';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AlertComponent, AlertArg } from '../../shared/alert-dialog';
import { RecoveryCodeComponent } from './recovery-code.component';

@Component({
  selector: 'app-two-factor-auth',
  templateUrl: './two-factor-auth.component.html',
  styleUrls: ['./two-factor-auth.component.css']
})
export class TwoFactorAuthComponent implements OnInit {

  constructor(public dialog: MatDialog, public snackBar: MatSnackBar, private authService: AuthService, private route: ActivatedRoute) { }

  hasAuthenticator: boolean;
  recoveryCodesLeft: number;
  is2faEnabled: boolean;
  isMachineRemembered: boolean;

  ngOnInit() {
    this.route.data.subscribe(
      (data: { model: TwoFactorAuthenticationModel }) => {
        this.model = data.model;
      })
  }

  set model(m: TwoFactorAuthenticationModel) {
    this.hasAuthenticator = m.hasAuthenticator;
    this.recoveryCodesLeft = m.recoveryCodesLeft;
    this.isMachineRemembered = m.isMachineRemembered;
    this.is2faEnabled = m.is2faEnabled;
  }

  forgetClient() {
    this.dialog.open<AlertComponent, AlertArg, boolean>(AlertComponent,
      { data: { title: "忘记本设备", message: "忘记设备后，下次密码登陆将需要重新输入两步验证码。" } })
      .afterClosed().subscribe(ok => {
        if (ok) {
          this.authService.forgetClient().subscribe(ok => {
            if (ok) {
              this.isMachineRemembered = false;
              this.snackBar.open("操作成功。", null, { duration: 3000 });
            } else {
              this.snackBar.open("操作失败，请刷新重试。", null, { duration: 3000 });
            }
          })
        }
      })
  }

  disable2fa(reset: boolean) {
    let data: AlertArg = {
      title: (reset ? '重设' : '禁用') + '两步验证',
      message: reset ? '重设后，您需要重新启用两步验证，原有的两步验证APP绑定将全部作废。'
        : '暂时禁用两步验证，您可以使用已有的两步验证APP重新激活。如果希望解除原有的两步验证APP绑定，请点击“重设验证秘钥”按钮。'
    }
    this.dialog.open<AlertComponent, AlertArg, boolean>(AlertComponent, { data: data })
      .afterClosed().subscribe(ok => {
        if (ok) {
          this.authService.disable2Fa(reset).subscribe(ok => {
            if (ok) {
              this.authService.get2FaData().subscribe(m => this.model = m);
              this.snackBar.open("操作成功。", null, { duration: 3000 });
            } else {
              this.snackBar.open("操作失败，请刷新重试。", null, { duration: 3000 });
            }
          })
        }
      })
  }

  generateRecoveryCodes() {
    this.dialog.open<AlertComponent, AlertArg, boolean>(AlertComponent,
      { data: { title: "重设应急密码", message: "重设后，原有的应急密码将全部作废。" } })
      .afterClosed().subscribe(ok => {
        if (ok) {
          this.authService.generateRecoveryCodes().subscribe(codes => {
            this.dialog.open<RecoveryCodeComponent, string[]>(RecoveryCodeComponent, { data: codes, disableClose: true })
          })
        }
      })
  }
}
