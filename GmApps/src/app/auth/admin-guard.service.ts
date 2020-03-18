import { Observable } from "rxjs/Observable";
import { AuthGuard } from "./auth-guard.service";

export class AdminGuard extends AuthGuard {
    checkLogin(url: string): Observable<boolean> {
        return this.authService.getUser().map(u => u.isAdmin())
            .do(authenticated => {
                if (authenticated) {
                    return;
                }

                // Store the attempted URL for redirecting
                this.authService.redirectUrl = url;

                // Navigate to the login page with extras
                this.router.navigate(["/login", { fragment: "role"}]);
                return;
            }, err => {
                this.router.navigate(["/login"], { fragment: "error" });
            });
    }
}
