import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import * as moment from 'moment-timezone';
import { Paged } from '../../models/Paged';
import { RaffleConfig } from '../../models/RaffleConfig';
import { AdminService, DraftResult } from '../admin.service';
import { DraftResultComponent } from './draft-result.component';

@Component({
  selector: 'app-raffle',
  templateUrl: './raffle.component.html',
  styleUrls: ['./raffle.component.css']
})
export class RaffleComponent implements OnInit {

  constructor(
    private service: AdminService,
    private snackbar: MatSnackBar,
    private dialog: MatDialog
  ) { }

  loading = true;
  configs: Paged<RaffleConfig>;
  page = 1;
  newCfg: RaffleConfig = {
    id: 0,
    eventStart: new Date(),
    eventEnd: moment().add(7, "days").toDate(),
    raffleCost: 200,
    title: "",
    image: "",
  }

  ngOnInit(): void {
    this.service.allRaffles(this.page).subscribe(c => {
      this.configs = c;
      this.loading = false;
    });
  }

  dateOnly(cfg: RaffleConfig) {
    cfg.eventStart = moment(cfg.eventStart).utc().startOf('date').toDate();
    cfg.eventEnd = moment(cfg.eventEnd).utc().endOf('date').subtract(1, 'second').toDate();
  }

  update(cfg: RaffleConfig) {
    this.dateOnly(cfg);
    this.loading = true;
    this.service.updateRaffle(cfg).subscribe(
      _ => {
        this.snackbar.open("已保存", null, { duration: 3000 });
        this.ngOnInit();
      },
      _ => {
        this.snackbar.open("更新失败，请重试", null, { duration: 3000 })
        this.loading = false;
      }
    );
  }

  add(cfg: RaffleConfig) {
    this.dateOnly(cfg);
    this.loading = true;
    this.service.addRaffle(cfg).subscribe(
      _ => {
        this.snackbar.open("已保存", null, { duration: 3000 });
        this.ngOnInit();
      },
      _ => {
        this.snackbar.open("更新失败，请重试", null, { duration: 3000 })
        this.loading = false;
      }
    );
  }

  draft(cfg: RaffleConfig) {
    this.loading = true;
    this.service.draftRaffle(cfg.id).subscribe(
      r => {
        this.dialog.open<DraftResultComponent, DraftResult>(DraftResultComponent, { data: r, width: "80vw" });
        this.loading = false;
      },
      _ => {
        this.snackbar.open("抽奖失败，请重试", null, { duration: 3000 })
        this.loading = false;
      }
    );
  }
}
