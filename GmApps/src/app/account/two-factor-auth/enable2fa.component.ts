import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ScriptLoaderService } from '../../shared/script-loader.service';
import { AuthService } from '../../auth/auth.service';
import { forkJoin } from 'rxjs';
import * as QRCode from 'qrcode';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RecoveryCodeComponent } from './recovery-code.component';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-enable2fa',
  templateUrl: './enable2fa.component.html',
  styleUrls: ['./enable2fa.component.css']
})
export class Enable2faComponent implements OnInit {

  constructor(
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
    private scriptLoader: ScriptLoaderService,
    private authService: AuthService,
    private router: Router) { }

  authUri: string
  key: string
  loading: boolean
  @ViewChild("qr") qrcodeCanvas: ElementRef<HTMLCanvasElement>
  code: string

  ngOnInit() {
    this.loading = true
    forkJoin(this.scriptLoader.loadScript('qrcode'), this.authService.get2FaKeys())
      .subscribe(([_, key]) => {
        this.authUri = key.authenticatorUri;
        this.key = key.sharedKey;
        this.loading = false;
        QRCode.toCanvas(this.qrcodeCanvas.nativeElement, this.authUri);
      });
  }

  verifyCode() {
    this.authService.enable2Fa(this.code).subscribe(ret => {
      this.snackBar.open("两步验证已成功启用。", null, { duration: 3000 });
      if (ret) {
        this.dialog.open<RecoveryCodeComponent, string[]>(RecoveryCodeComponent, { data: ret, disableClose: true })
          .afterClosed().subscribe(_ => this.router.navigate(['/account', '2fa']))
      } else {
        this.router.navigate(['/account', '2fa'])
      }
    }, (err: HttpErrorResponse) => {
      this.snackBar.open(err.status == 401 ? "登陆信息已失效，请刷新重试。" : "验证码有误，请重试。", null, { duration: 3000 });
    })
  }
}
