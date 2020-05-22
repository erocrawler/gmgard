import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { Observable } from 'rxjs';
import { IVoucher, VoucherKind, PrizeInfo } from '../../models/Vouchers';
import { MatTableDataSource, MatTable } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { WheelService } from '../wheel.service';
import { MatDialog } from '@angular/material/dialog';
import { AlertComponent, AlertArg } from '../../shared/alert-dialog';
import { alertDialog } from '../../shared/alert-dialog/alert.component';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'voucher-list',
  templateUrl: './voucher-list.component.html',
  styleUrls: ['./voucher-list.component.css']
})
export class VoucherListComponent implements OnInit {

  constructor(private wheelService: WheelService, private dialog: MatDialog, private snackBar: MatSnackBar) { }

  @Input() displayedColumns: string[];
  @Input() vouchers: Observable<IVoucher[]>;
  @Input() prizeInfo: PrizeInfo[];
  @Output() change = new EventEmitter<IVoucher>(true);

  VoucherKindType = VoucherKind;
  dataSource = new MatTableDataSource<IVoucher>();
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild(MatTable, { static: true }) table: MatTable<IVoucher>;

  ngOnInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.vouchers.subscribe(v => {
      this.dataSource.data = v;
    })
  }

  voucherKind(v: IVoucher): string {
    switch (v.kind) {
      case VoucherKind.WheelA:
        return "棒棒糖抽奖"
      case VoucherKind.WheelB:
        return "幸运积分抽奖"
      case VoucherKind.LuckyPoint:
        return "幸运积分券"
      case VoucherKind.Prize:
        return "奖品兑换券"
      case VoucherKind.CeilingPrize:
        return "特别兑换券"
      case VoucherKind.Coupon:
        return "优惠券"
    }
    return "";
  }

  canExchange(v: IVoucher): boolean {
    if (v.kind != VoucherKind.Prize && v.kind != VoucherKind.Coupon) {
      return false;
    }
    return this.exchangeValue(v) > 0;
  }

  exchangeValue(v: IVoucher): number {
    if (this.prizeInfo != null) {
      const parenRe = /（.+）/;
      const prizeName = v.redeemItem.replace(parenRe, "");
      let p = this.prizeInfo.find(p => p.prizeName == prizeName);
      if (p) {
        return p.prizeLPValue;
      }
    }
    return 0;
  }

  exchange(v: IVoucher) {
    alertDialog(this.dialog, { title: "奖品折换", message: `确认将${v.redeemItem}折换为${this.exchangeValue(v)}幸运积分？折换后不可撤销。` }).subscribe(r => {
      if (!r) {
        return;
      }
      this.wheelService.exchange(v.voucherID).subscribe(s => {
        this.snackBar.open("折换成功", null, { duration: 3000 });
        this.change.emit(s);
      })
    })
  }

  markUsed(v: IVoucher) {
    alertDialog(this.dialog, { title: "标记使用", message: `确认将${v.redeemItem || this.voucherKind(v)}标记为已使用状态？` }).subscribe(r => {
      if (!r) {
        return;
      }
      this.wheelService.markRedeemed(v.voucherID).subscribe(_ => {
        this.snackBar.open("标记成功", null, { duration: 3000 });
        this.change.emit(v);
      })
    });
  }
}
