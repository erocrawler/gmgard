import { Component, OnInit, ViewChild } from '@angular/core';
import { WheelService } from '../wheel.service';
import { StockInfo, IVoucher, VoucherKind, newVoucher } from '../../models/Vouchers';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subject } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

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
  loading = false;

  ngOnInit(): void {
    this.route.params.map(p => {
      return {
        mode: String(p['mode'] || ''),
        userName: String(p['name'] || ''),
      };
    }).subscribe(params => {
      if (params.mode == "user") {
        this.displayMode = 'user'
        this.userLookupName = params.userName;
        if (this.userLookupName) {
          this.getForUser();
        }
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

  getStock() {
    this.loading = true;
    this.wheelService.getStock().subscribe(s => {
      this.loading = false;
      this.stocks = s;
      this.vouchers.next(s.filter(v => v.stock).map(v => v.stock.map(newVoucher)).reduce((acc, val) => acc.concat(val), []));
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
