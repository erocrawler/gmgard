import { Component, OnInit } from '@angular/core';
import { WheelService } from '../wheel.service';
import { IVoucher, LuckyPointVoucher, VoucherKind } from '../../models/Vouchers';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-vouchers',
  templateUrl: './vouchers.component.html',
  styleUrls: ['./vouchers.component.css']
})
export class VouchersComponent implements OnInit {

  constructor(private wheelService: WheelService) { }

  displayedColumns: string[] = ['id', 'kind', 'redeemItem', 'issueTime', 'useTime', 'exchange'];
  vouchers = new Subject<IVoucher[]>();
  luckyPoints: number;
  userPoints: number;
  loading = false;

  ngOnInit(): void {
    this.updateStatus();
  }

  updateStatus() {
    this.loading = true;
    this.wheelService.getStatus().subscribe(s => {
      this.loading = false;
      this.vouchers.next(s.vouchers.filter(v => v.kind != VoucherKind.WheelA && v.kind != VoucherKind.WheelB));
      this.userPoints = s.userPoints;
      this.luckyPoints = s.vouchers
        .filter(v => v instanceof LuckyPointVoucher)
        .reduce((total, v: LuckyPointVoucher) => total + v.currentValue, 0);
    })
  }



}
