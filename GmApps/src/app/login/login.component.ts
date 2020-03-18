import { Inject, Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { MatSnackBar } from "@angular/material";

import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { AuthService, LoginResult } from "app/auth/auth.service";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent implements OnInit {

  host: string;
  username: string;
  password: string;
  rememberMe: boolean;
  captcha: string;
  error = false;
  errorMessage = "";
  twoFactorAuth: boolean;
  rememberMachine: boolean;
  twoFactorCode: string;
  useRecoveryCode: boolean;
  recoveryCode: string;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    @Inject(ENVIRONMENT) env: Environment,
    public snackBar: MatSnackBar) {
    this.host = env.apiHost;
  }

  ngOnInit() {
    this.route
      .fragment
      .subscribe(fragment => {
        setTimeout(() => {
          if (fragment === "error") {
            this.snackBar.open("登录验证失败，请刷新重试。", null, { duration: 3000 });
          } else if (fragment === "role") {
            this.snackBar.open("您没有权限访问此页面。", null, { duration: 3000 });
          }
        })
      });
  }

  login() {
    this.error = false;
    this.authService.login(this.username, this.password, this.rememberMe, this.captcha).subscribe(result => {
      if (result.require2fa && result.success) {
        this.twoFactorAuth = true;
      } else {
        this.handleResult(result);
      }
    })
  }

  reset() {
    this.error = false;
    this.useRecoveryCode = false;
    this.twoFactorAuth = false;
    this.username = '';
    this.password = '';
    this.rememberMe = false;
    this.rememberMachine = false;
    this.captcha = '';
  }

  twoFactorLogin() {
    this.error = false;
    this.authService.twoFactorAuthLogin(this.rememberMe, this.rememberMachine, this.twoFactorCode).subscribe(result => {
      this.handleResult(result);
    })
  }

  doRecoveryCode() {
    this.useRecoveryCode = true;
  }

  recoveryCodeLogin() {
    this.error = false;
    this.authService.recoveryCodeLogin(this.recoveryCode).subscribe(result => {
      this.handleResult(result);
    })
  }

  private handleResult(result: LoginResult) {
    if (result.success) {
      this.router.navigateByUrl(this.authService.redirectUrl || "/");
    } else {
      this.error = true;
      this.errorMessage = result.errors.join(";");
    }
  }
}
