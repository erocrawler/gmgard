import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SpinWheelResult } from '../../models/Vouchers';
import { ClipboardService } from 'ngx-clipboard';

@Component({
  selector: 'app-prize-confirm',
  templateUrl: './prize-confirm.component.html',
  styleUrls: ['./prize-confirm.component.css']
})
export class PrizeConfirmComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: SpinWheelResult, private clipboard: ClipboardService) { }

  ngOnInit(): void {
  }

  copyVoucher() {
    this.clipboard.copyFromContent(this.data.voucher.voucherID);
  }

  get isEmpty(): boolean {
    return !this.data.prize.isRealItem && !this.data.prize.isVoucher
  }

}
