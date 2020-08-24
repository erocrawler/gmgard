import { Component, OnInit } from '@angular/core';
import { WheelService } from '../wheel.service';
import { StockInfo, IVoucher, newVoucher } from '../../models/Vouchers';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

interface Statistics {
  total: number;
  stocks: {
    prizeName: string;
    total: number;
    assigned: number;
    used: number;
    unassigned: number;
    totalDrawCount: number;
    manualExchanged: number;
  }[];
}

@Component({
  selector: 'app-stock',
  templateUrl: './stock.component.html',
  styleUrls: ['./stock.component.css']
})
export class StockComponent implements OnInit {

  constructor(private wheelService: WheelService, private snackBar: MatSnackBar, private route: ActivatedRoute, private router: Router) { }
  stocks: StockInfo[];
  displayedColumns: string[] = ['id', 'kind', 'redeemItem', 'issueTime', 'userName', 'useTime',  'markUsed'];
  vouchers = new Subject<IVoucher[]>();
  addPrizeName: string;
  addCount = 0;
  displayMode = 'stock';
  userLookupName: string;
  voucherLookup: string;
  loading = false;
  statistics: Statistics;

  ngOnInit(): void {
    this.route.params.pipe(map(p => {
      return {
        mode: String(p['mode'] || ''),
        userName: String(p['name'] || ''),
      };
    })).subscribe(params => {
      if (params.mode == "user") {
        this.displayMode = 'user'
        this.userLookupName = params.userName;
        if (this.userLookupName) {
          this.getForUser();
        }
      } else if (params.mode == "voucher") {
        this.displayMode = 'voucher'
      } else {
        this.displayMode = 'stock'
        this.getStock();
      }
    })
  }

  updateUrl() {
    this.router.navigate(["wheel", "admin", { "mode" : this.displayMode }])
  }

  updateList() {
    if (this.displayMode == "user") {
      this.getForUser();
    } else {
      this.getStock();
    }
  }

  getForUser() {
    this.loading = true;
    this.wheelService.getForUser(this.userLookupName).subscribe(s => {
      this.loading = false;
      this.vouchers.next(s.map(newVoucher));
    }, (err: HttpErrorResponse) => {
      this.loading = false;
        let msg = "查询失败";
        if (err.status === 404) {
          msg = "查无此人，请检查输入的用户名";
        }
      this.snackBar.open(msg, null, { duration: 3000 });
    });
  }

  getVoucher() {
    this.loading = true;
    this.wheelService.getVoucher(this.voucherLookup).subscribe(s => {
      this.loading = false;
      this.vouchers.next(s.map(newVoucher));
    }, (err: HttpErrorResponse) => {
      this.loading = false;
      let msg = "查询失败";
      if (err.status === 404) {
        msg = "查无此券，请检查输入的奖券ID";
      }
      this.snackBar.open(msg, null, { duration: 3000 });
    });
  }

  getStock() {
    this.loading = true;
    this.wheelService.getStock().subscribe(s => {
      this.loading = false;
      this.stocks = s;
      let allVouchers = s.filter(v => v.stock).map(v => v.stock.map(newVoucher)).reduce((acc, val) => acc.concat(val), []);
      this.vouchers.next(allVouchers);
      this.statistics = {
        total: allVouchers.length,
        stocks: this.stocks.filter(v => v.stock).map(s => {
          return {
            prizeName: s.prizeName,
            total: s.stock.length,
            assigned: s.stock.filter(s => s.userName).length,
            used: s.stock.filter(s => s.useTime).length,
            unassigned: s.stock.filter(s => !s.userName).length,
            totalDrawCount: s.totalDrawCount,
            manualExchanged: s.manualExchangedCount,
          }
        })
      }
    });
  }

  addStock(prizeName: string, count: number) {
    if (!prizeName || count < 1) {
      return;
    }
    this.wheelService.addStock(prizeName, count).subscribe(_ => {
      this.snackBar.open("添加成功", null, { duration: 3000 });
      this.getStock();
    });
  }
}
