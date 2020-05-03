import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { WheelComponent } from './wheel.component';
import { AdManagerGuard } from '../auth/admanager-guard.service';
import { WheelIndexComponent } from './wheel-index.component';
import { StockComponent } from './admin/stock.component';
import { WheelService } from './wheel.service';
import { AppMaterialModule } from '../app-material.module';
import { VouchersComponent } from './vouchers/vouchers.component';
import { UserResolverService } from '../auth/user-resolver.service';
import { MatTableModule } from '@angular/material/table';
import { FormsModule } from '@angular/forms';
import { SpinConfirmComponent } from './dialogs/spin-confirm.component';
import { MatDialogModule } from '@angular/material/dialog';
import { RedeemPointsComponent } from './dialogs/redeem-points.component';
import { RedeemCeilingComponent } from './dialogs/redeem-ceiling.component';
import { PrizeConfirmComponent } from './dialogs/prize-confirm.component';
import { VoucherListComponent } from './vouchers/voucher-list.component';
import { MatSortModule } from '@angular/material/sort';

const routes: Routes = [
  {
    path: "",
    component: WheelComponent,
    resolve: {
      user: UserResolverService
    },
    children: [
      { path: "", component: WheelIndexComponent, pathMatch: "full" },
      { path: "vouchers", component: VouchersComponent },
      { path: "admin", component: StockComponent, canActivate: [AdManagerGuard] },
    ]
  },

];

@NgModule({
  declarations: [
    WheelComponent,
    WheelIndexComponent,
    StockComponent,
    VouchersComponent,
    SpinConfirmComponent,
    RedeemPointsComponent,
    RedeemCeilingComponent,
    PrizeConfirmComponent,
    VoucherListComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes),
    AppMaterialModule,
    MatTableModule,
    MatDialogModule,
    MatSortModule,
  ],
  providers: [
    WheelService,
  ]
})
export class WheelModule { }
