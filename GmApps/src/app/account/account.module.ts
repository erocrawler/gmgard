import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TwoFactorAuthComponent } from './two-factor-auth/two-factor-auth.component';
import { AccountComponent } from './account.component';
import { Routes, RouterModule } from '@angular/router';
import { AppMaterialModule } from '../app-material.module';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule } from '@angular/forms';
import { TwoFactorAuthDataResolverService } from './two-factor-auth/two-factor-auth-data-resolver.service';
import { RecoveryCodeComponent } from './two-factor-auth/recovery-code.component';
import { Enable2faComponent } from './two-factor-auth/enable2fa.component';
import { MatDialogModule } from '@angular/material';
import { AlertModule } from '../shared/alert-dialog';

const routes: Routes = [
  {
    path: "",
    component: AccountComponent,
    children: [
      { path: "", pathMatch: "full", redirectTo: "2fa" },
      {
        path: "2fa",
        component: TwoFactorAuthComponent,
        resolve: {
          model: TwoFactorAuthDataResolverService
        }
      },
      {
        path: "enable2fa",
        component: Enable2faComponent,
      },
    ]
  },
];

@NgModule({
  imports: [
    AppMaterialModule,
    CommonModule,
    FlexLayoutModule,
    MatDialogModule,
    FormsModule,
    AlertModule,
    RouterModule.forChild(routes)
  ],
  entryComponents: [RecoveryCodeComponent],
  declarations: [TwoFactorAuthComponent, AccountComponent, RecoveryCodeComponent, Enable2faComponent]
})
export class AccountModule { }
