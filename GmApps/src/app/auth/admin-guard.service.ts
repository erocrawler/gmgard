import { AuthGuard } from "./auth-guard.service";
import { Injectable } from "@angular/core";
import { User } from "../models/User";

@Injectable()
export class AdminGuard extends AuthGuard {
  checkRole(u: User): boolean {
    return u.isAdmin();
  }
}
