import { Component, ViewChild, AfterViewInit, OnInit } from '@angular/core';
import { MatCalendar } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PunchInDetailComponent } from './punch-in-detail.component';
import { PunchInService, PunchInHistory } from './punch-in.service';
import { User } from 'app/models/User';
import { ActivatedRoute } from '@angular/router';
import { Subscription, of } from 'rxjs';
import * as moment from 'moment';
import { delay } from 'rxjs/operators';

@Component({
  templateUrl: './punch-in.component.html',
  styleUrls: ['./punch-in.component.css']
})
export class PunchInComponent implements AfterViewInit, OnInit {

  constructor(public dialog: MatDialog, public snackBar: MatSnackBar, private service: PunchInService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.activeDate = moment();
    this.getMonthData();
    this.route.data.subscribe((d: { user: User }) => {
      this.user = d.user;
      this.points = d.user.points;
      this.consecutiveSign = d.user.consecutiveSign;
    });
  }

  ngAfterViewInit() {
    this.calendar.stateChanges.subscribe(() => {
      if (this.calendar.currentView != 'month') {
        return;
      }
      if (this.calendar.activeDate && !this.calendar.activeDate.isSame(this.activeDate, 'month')) {
        this.activeDate = this.calendar.activeDate;
        this.getMonthData();
      }
    });
    this.calendar.monthSelected.subscribe((d: moment.Moment) => {
      this.activeDate = d;
      this.getMonthData();
    })
  }

  getMonthData() {
    let d = this.activeDate;
    let m = d.get('month')+1
    let y = d.get('year');
    this.loading = true;
    if (this.monthDataSub && !this.monthDataSub.closed) {
      this.monthDataSub.unsubscribe();
    }
    this.monthDataSub = this.service.history(y, m).subscribe(
      d => {
        this.histories = d.punchIns;
        this.legacySign = d.legacySignDays;
        if (d.minSignDate) {
          let mo = moment(d.minSignDate);
          if (!mo.isSame(this.minDate, 'day')) {
            this.minDate = mo;
          }
        }
        of(1).pipe(delay(1)).subscribe(() => this.updateDateFilter(this.histories));
        this.loading = false;
        },
      () => {
        this.snackBar.open("数据读取出错，请刷新重试。", null, { duration: 3000 });
        this.loading = false;
      });
  }

  updateDateFilter(histories: PunchInHistory[]) {
    let hset = new Set<Number>();
    for (let h of histories) {
      let t = moment(h.timeStamp);
      hset.add(t.date());
    }
    let c = document.querySelectorAll('.mat-calendar-body-cell');
    for (let i = 0; i < c.length; i++) {
      if (c.item(i).classList.contains('mat-calendar-body-disabled')) {
        continue;
      }
      if (!hset.has(i + 1)) {
        c.item(i).classList.add('sign-missed');
      } else {
        c.item(i).classList.remove('sign-missed');
      }
    }
  }

  onSelect(d: moment.Moment) {
    if (this.histories.find(h => d.isSame(moment(h.timeStamp), 'date'))) {
      return;
    }
    let diag = this.dialog.open<PunchInDetailComponent, moment.Moment, string>(PunchInDetailComponent, { data: d });
    diag.beforeClosed().subscribe(mode => {
      if (!mode) {
        return;
      }
      this.loading = true;
      this.service.punchIn(d, mode === "ticket").subscribe(r => {
        this.getMonthData();
        this.consecutiveSign = r.consecutiveDays;
        this.points = r.points;
        if (r.success) {
          this.snackBar.open(r.expBonus ? `签到成功，棒棒糖+${r.expBonus}， 绅士度+${r.expBonus}` : "签到成功。", null, { duration: 3000 });
        } else {
          this.snackBar.open(`签到失败：${r.errorMessage}`, null, { duration: 3000 });
        }
      }, () => {
        this.snackBar.open("签到出错，请刷新重试。", null, { duration: 3000 });
        this.loading = false;
      });
    })
  }

  monthDataSub: Subscription;
  activeDate: moment.Moment;
  loading = true;
  dateClasses: (date: moment.Moment) => string;
  minDate: moment.Moment = moment(new Date(2019, 0, 25));
  maxDate: moment.Moment = moment();
  @ViewChild(MatCalendar) calendar: MatCalendar<moment.Moment>;
  user: User;
  histories: PunchInHistory[];
  consecutiveSign = 0;
  points = 0;
  legacySign = 0;
}
