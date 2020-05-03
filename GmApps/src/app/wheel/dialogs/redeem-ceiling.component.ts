import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { IVoucher } from '../../models/Vouchers';
import { ClipboardService } from 'ngx-clipboard';

@Component({
  selector: 'app-redeem-ceiling',
  templateUrl: './redeem-ceiling.component.html',
  styleUrls: ['./redeem-ceiling.component.css']
})
export class RedeemCeilingComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: IVoucher, private clipboard: ClipboardService) { }

  ngOnInit(): void {
  }

  copyVoucher() {
    this.clipboard.copyFromContent(this.data.voucherID);
  }

}
