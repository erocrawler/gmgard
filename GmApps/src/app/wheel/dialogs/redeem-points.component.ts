import { Component, OnInit } from '@angular/core';
import { WheelService } from '../wheel.service';
import { HttpErrorResponse } from '@angular/common/http';
import { PrizeInfo } from '../../models/Vouchers';
import { ShowOnDirtyErrorStateMatcher, ErrorStateMatcher } from '@angular/material/core';
import { MatDialogRef } from '@angular/material/dialog';

class RedeemErrMatcher implements ErrorStateMatcher {

  err = ""
  isErrorState(_): boolean {
      return !!this.err
  }

}

@Component({
  selector: 'app-redeem-points',
  templateUrl: './redeem-points.component.html',
  styleUrls: ['./redeem-points.component.css']
})
export class RedeemPointsComponent implements OnInit {

  constructor(private service: WheelService, public matDialogRef: MatDialogRef<RedeemPointsComponent>) { }

  ngOnInit(): void {
    this.matDialogRef.beforeClosed().subscribe(() => this.matDialogRef.close(this.hasRedeemed));
  }

  errMatcher = new RedeemErrMatcher();
  voucherInput: string
  prize: PrizeInfo
  hasRedeemed = false;

  clearErr() {
    this.errMatcher.err = "";
  }

  redeem() {
    this.errMatcher.err = "";
    this.prize = null;
    this.service.redeemPoints(this.voucherInput).subscribe(p => {
      this.hasRedeemed = true;
      this.prize = p;
    }, (err: HttpErrorResponse) => {
        if (err.status == 400) {
          this.errMatcher.err = "无效的兑换码。";
        } else {
          this.errMatcher.err = "兑换失败，请重试。"
        }
    })
  }

}
