import { Injectable } from '@angular/core';
import { TwoFactorAuthenticationModel } from 'app/models/TwoFactorAuth';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { Observable } from 'rxjs';
import { first } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class TwoFactorAuthDataResolverService implements Resolve<TwoFactorAuthenticationModel> {

  constructor(private as: AuthService) { }

  resolve(_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<TwoFactorAuthenticationModel> {
    return this.as.get2FaData().pipe(
      first()
    );
  }
}
