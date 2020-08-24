import { Observable, of, concat } from "rxjs";
import { map, switchMap, tap } from "rxjs/operators";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { AntiForgeryToken } from "../models/AntiForgeryToken";
import { User, IUser } from "../models/User";
import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { TwoFactorAuthenticationModel, TwoFactorAuthSharedKey } from "../models/TwoFactorAuth";

export interface LoginResult {
  success: boolean,
  require2fa?: boolean,
  errors: string[],
}

@Injectable()
export class AuthService {
  private host: string;
  private isLoggedIn?: boolean;
  private user: User;

  constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
    this.host = env.apiHost;
    this.isLoggedIn = null;
  }

  redirectUrl: string;
  antiforgeryToken: AntiForgeryToken;

  isAuthenticated(): Observable<boolean> {
    if (this.isLoggedIn === null) {
      return this.http.get<{ isAuthenticated: boolean }>(this.host + "/api/Account/IsAuthenticated", { withCredentials: true })
        .pipe(map(resp => this.isLoggedIn = resp.isAuthenticated as boolean));
    }
    return of(this.isLoggedIn);
  }

  getAntiforgeryToken(): Observable<AntiForgeryToken> {
    if (this.antiforgeryToken) {
      return of(this.antiforgeryToken);
    }
    return this.http.get<AntiForgeryToken>(this.host + "/api/Account/GetAntiForgeryToken", { withCredentials: true });
  }

  getUser(force: boolean = false): Observable<User> {
    let ret: Observable<User>;
    if (this.user && !force) {
      return of(this.user);
    }
    ret = this.isAuthenticated()
      .pipe(switchMap(loggedIn => {
        if (loggedIn) {
          return this.http.get<IUser>(this.host + "/api/Account/GetUser", { withCredentials: true })
            .pipe(map(u => new User(u)));
        }
        return of(null);
      }), tap(u => this.user = u));
    if (this.user) {
      ret = concat(of(this.user), ret);
    }
    return ret;
  }

  login(user: string, password: string, remember: boolean, captcha: string): Observable<LoginResult> {
    const req = { userName: user, password: password, rememberMe: remember, captcha };
    return this.http.post<LoginResult>(this.host + "/api/Account/Login", req, { withCredentials: true })
      .pipe(tap(result => {
        if (result.success && !result.require2fa) {
          this.isLoggedIn = true;
        }
      }));
  }

  logout(): Observable<void> {
    return this.getAntiforgeryToken().pipe(
      switchMap(token => {
        const req = {};
        req[token.fieldName] = token.value;
        return this.http.post(this.host + "/api/Account/LogOut", req, { withCredentials: true });
      }), map(_ => {
        this.isLoggedIn = false;
        this.user = null;
      }));
  }

  twoFactorAuthLogin(rememberMe: boolean, rememberMachine: boolean, twoFactorCode: string): Observable<LoginResult> {
    const req = { rememberMe, rememberMachine, twoFactorCode }
    return this.http.post<LoginResult>(this.host + "/api/Account/TwoFactorAuth", req, { withCredentials: true })
      .pipe(tap(result => {
        if (result.success) {
          this.isLoggedIn = true;
        }
      }));
  }

  recoveryCodeLogin(recoveryCode: string) {
    return this.http.post<LoginResult>(this.host + "/api/Account/RecoveryCode", { recoveryCode }, { withCredentials: true })
      .pipe(tap(result => {
        if (result.success) {
          this.isLoggedIn = true;
        }
      }));
  }

  get2FaData(): Observable<TwoFactorAuthenticationModel> {
    return this.http.get<TwoFactorAuthenticationModel>(this.host + "/api/Account/Manage2Fa", { withCredentials: true });
  }

  get2FaKeys(): Observable<TwoFactorAuthSharedKey> {
    return this.http.get<TwoFactorAuthSharedKey>(this.host + "/api/Account/Get2FaKeys", { withCredentials: true });
  }

  enable2Fa(code: string): Observable<string[]> {
    return this.http.post<string[]>(this.host + "/api/Account/Enable2Fa", null, { params: { code }, withCredentials: true });
  }

  disable2Fa(reset: boolean): Observable<boolean> {
    return this.http.post(this.host + "/api/Account/Disable2Fa", null, { params: { "reset": reset.toString() }, observe: "response", withCredentials: true })
      .pipe(map(resp => {
        return resp.ok;
      }));
  }

  forgetClient(): Observable<boolean> {
    return this.http.post(this.host + "/api/Account/ForgetClient", null, { observe: "response", withCredentials: true })
      .pipe(map(resp => resp.ok));
  }

  generateRecoveryCodes(): Observable<string[]> {
    return this.http.post<string[]>(this.host + "/api/Account/GenerateRecoveryCodes", null, { withCredentials: true });
  }
}
