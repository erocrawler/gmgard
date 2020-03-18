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
import "rxjs/add/operator/do";

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

    checkLogin(url: string): Observable<boolean> {
        return this.authService.isAuthenticated().do(authenticated => {
            if (authenticated) {
                return;
            }

            // Store the attempted URL for redirecting
            this.authService.redirectUrl = url;

            // Navigate to the login page with extras
            this.router.navigate(["/login"]);
            return;
        }, err => {
            this.router.navigate(["/login"], { fragment: "error" });
        });
    }

    canLoad(route: Route): Observable<boolean> {
        const url = `/${route.path}`;

        return this.checkLogin(url);
    }
}
