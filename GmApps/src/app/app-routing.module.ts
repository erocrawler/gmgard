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
import { AdManagerGuard } from "./auth/admanager-guard.service";
import { AppLayoutComponent } from "./app-layout.component";

const routes: Routes = [
  { path: "", redirectTo: "/title-helper", pathMatch: "full" },
  { path: "raffle", component: RaffleIndexComponent, data: { title: "绅士彩券" }, canActivate: [AuthGuard] },
  { path: "account", loadChildren: () => import('app/account/account.module').then(m => m.AccountModule), data: { title: "账户管理" }, canActivate: [AuthGuard] },
  {
    path: "message", loadChildren: () => import('app/message/message.module').then(m => m.MessageModule), canActivate: [AuthGuard]
  },
  {
    path: "",
    component: AppLayoutComponent,
    children: [
      { path: "title-helper", component: TitleHelperComponent, data: { title: "标题助手" }, canActivate: [AuthGuard] },
      {
        path: "audit-exam",
        loadChildren: () => import("app/audit-exam/audit-exam.module").then(m => m.AuditExamModule),
        data: { title: "审核组测试", toolbar: AuditExamToolbarComponent },
        canActivate: [AuthGuard]
      },
      {
        path: "bounty",
        loadChildren: () => import("app/bounty/bounty.module").then(m => m.BountyModule),
        data: { title: "绅士求物板" },
        canActivate: [AuthGuard]
      },
      {
        path: "gacha",
        loadChildren: () => import("app/gacha/gacha.module").then(m => m.GachaModule),
        data: { title: "绅士抽奖" },
        canActivate: [AuthGuard]
      },
      {
        path: "game",
        loadChildren: () => import("app/game/game.module").then(m => m.GameModule),
        data: { title: "绅士冒险" },
        canActivate: [AuthGuard]
      },
      {
        path: "admin",
        loadChildren: () => import("app/admin/admin.module").then(m => m.AdminModule),
        data: { title: "绅士管理" },
        canLoad: [AdminGuard]
      },
      {
        path: "punch-in",
        loadChildren: () => import("app/punch-in/punch-in.module").then(m => m.PunchInModule),
        data: { title: "绅士签到" },
        canActivate: [AuthGuard]
      },
      {
        path: "wheel",
        loadChildren: () => import("app/wheel/wheel.module").then(m => m.WheelModule),
        data: { title: "绅士转盘" },
        canActivate: [AuthGuard]
      },
      { path: "login", component: LoginComponent },
      { path: "**", component: PageNotFoundComponent },
    ],
  }

];
@NgModule({
  imports: [
    RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })
  ],
  exports: [RouterModule],
  providers: [
    AdManagerGuard,
    AdminGuard,
    AuthGuard,
    AuthService,
  ],
})
export class AppRoutingModule { }
