import { Injectable } from "@angular/core";
import {
  CanActivate, CanLoad, Router, Route,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  CanActivateChild,
  NavigationExtras
} from "@angular/router";
import { AuthService } from "./auth.service";

import { Observable } from "rxjs/Observable";
import { of, combineLatest } from "rxjs";
import { map, catchError } from "rxjs/operators";
import { User } from "../models/User";

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {
  constructor(protected authService: AuthService, protected router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const url: string = state.url;

    return this.checkLogin(url);
  }

  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return this.canActivate(route, state);
  }

  checkRole(_: User): boolean {
    return true;
  }

  checkLogin(url: string): Observable<boolean> {
    return combineLatest(
      this.authService.isAuthenticated(),
      this.authService.getUser())
      .pipe(
        map(([authenticated, user]) => {
          if (authenticated && this.checkRole(user)) {
            return true;
          }

          // Store the attempted URL for redirecting
          this.authService.redirectUrl = url;

          let extra: NavigationExtras = {};
          if (authenticated && !this.checkRole(user)) {
            extra = { fragment: "role" };
          }

          // Navigate to the login page with extras
          this.router.navigate(["/login"], extra);
          return false;
        }),
        catchError(_ => {
          this.router.navigate(["/login"], { fragment: "error" });
          return of(false);
        })
      );
  }

  canLoad(route: Route): Observable<boolean> {
    const url = `/${route.path}`;

    return this.checkLogin(url);
  }
}
