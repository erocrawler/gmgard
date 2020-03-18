import { NgModule } from "@angular/core";
import { RouterModule, Router, Routes, PreloadAllModules } from "@angular/router";

import { AuditExamToolbarComponent } from "app/toolbar/audit-exam-toolbar.component";
import { PageNotFoundComponent } from "./page-not-found.component";
import { LoginComponent } from "./login/login.component"
import { TitleHelperComponent } from "./title-helper/title-helper.component";
import { AuthGuard } from "./auth/auth-guard.service";
import { AuthService } from "./auth/auth.service";
import { AdminGuard } from "./auth/admin-guard.service";
import { RaffleIndexComponent } from "./raffle/raffle-index.component";

const routes: Routes = [
  { path: "", redirectTo: "/title-helper", pathMatch: "full" },
  { path: "title-helper", component: TitleHelperComponent, data: { title: "标题助手" }, canActivate: [AuthGuard] },
  {
    path: "audit-exam",
    loadChildren: "app/audit-exam/audit-exam.module#AuditExamModule",
    data: { title: "审核组测试", toolbar: AuditExamToolbarComponent },
    canActivate: [AuthGuard]
  },
  { path: "bounty", loadChildren: "app/bounty/bounty.module#BountyModule", data: { title: "绅士求物板" }, canActivate: [AuthGuard] },
  { path: "gacha", loadChildren: "app/gacha/gacha.module#GachaModule", data: { title: "绅士抽奖" }, canActivate: [AuthGuard] },
  { path: "game", loadChildren: "app/game/game.module#GameModule", data: { title: "绅士冒险" }, canActivate: [AuthGuard] },
  { path: "admin", loadChildren: "app/admin/admin.module#AdminModule", data: { title: "绅士管理" }, canActivate: [AdminGuard] },
  { path: "account", loadChildren: "app/account/account.module#AccountModule", data: { title: "账户管理" }, canActivate: [AuthGuard] },
  { path: "punch-in", loadChildren: "app/punch-in/punch-in.module#PunchInModule", data: { title: "绅士签到" }, canActivate: [AuthGuard] },
  { path: "raffle", component: RaffleIndexComponent, data: { title: "绅士彩券" }, canActivate: [AuthGuard] },
  { path: "login", component: LoginComponent },
  { path: "**", component: PageNotFoundComponent },

];
@NgModule({
  imports: [
    RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })
  ],
  exports: [RouterModule],
  providers: [
    AdminGuard,
    AuthGuard,
    AuthService,
  ],
})
export class AppRoutingModule { }
