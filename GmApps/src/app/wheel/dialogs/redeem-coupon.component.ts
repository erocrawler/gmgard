import { Component, OnInit, Inject } from '@angular/core';
import { WheelService, StatusAndVouchers } from '../wheel.service';
import { PrizeInfo, SpinWheelResult, LuckyPointVoucher, SpinWheelStatus } from '../../models/Vouchers';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ClipboardService } from 'ngx-clipboard';
import { MatSnackBar } from '@angular/material/snack-bar';
import { zip } from 'rxjs/operators';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-redeem-coupon',
  templateUrl: './redeem-coupon.component.html',
  styleUrls: ['./redeem-coupon.component.css']
})
export class RedeemCouponComponent implements OnInit {

  selectedOption: number;
  couponOptions: PrizeInfo[];
  result: SpinWheelResult;
  points: number;
  loading = false;

  constructor(@Inject(MAT_DIALOG_DATA) public listener: Subject<StatusAndVouchers>, private service: WheelService, private clipboard: ClipboardService, private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.loading = true;
    this.service.getStatusAndVoucher().subscribe(r => {
      this.loading = false;
      this.couponOptions = r.status.couponPrizes;
      this.points = r.vouchers.filter(v => v instanceof LuckyPointVoucher).reduce((total, l: LuckyPointVoucher) => total + l.currentValue, 0);
      this.listener.next(r);
    })
  }

  redeem() {
    this.result = null;
    this.loading = true;
    this.service.redeemCoupon(this.selectedOption)
      .pipe(
        zip(this.service.getStatusAndVoucher())
      ).subscribe(([r, status]) => {
        this.loading = false;
        this.result = r;
        this.couponOptions = status.status.couponPrizes;
        this.points = status.vouchers.filter(v => v instanceof LuckyPointVoucher).reduce((total, l: LuckyPointVoucher) => total + l.currentValue, 0);
        this.listener.next(status);
      }, err => {
        this.loading = false;
        this.snackBar.open("兑换出错，请刷新重试！", null, { duration: 3000 });
      })
  }

  get canRedeem(): boolean {
    return !this.loading && this.selectedOption > 0 && this.points >= this.selectedOption;
  }

  copyVoucher() {
    let msg = this.clipboard.copyFromContent(this.result.voucher.voucherID) ? "已复制优惠券到粘贴板" : "复制失败，请手动复制";
    this.snackBar.open(msg, null, { duration: 3000 });
  }
}
