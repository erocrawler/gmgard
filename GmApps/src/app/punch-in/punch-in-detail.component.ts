import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PunchInService } from './punch-in.service';
import { Moment } from 'moment';

@Component({
  selector: 'app-punch-in-detail',
  templateUrl: './punch-in-detail.component.html',
  styleUrls: ['./punch-in-detail.component.css']
})
export class PunchInDetailComponent implements OnInit {

  constructor(
    public dialogRef: MatDialogRef<PunchInDetailComponent, string>,
    @Inject(MAT_DIALOG_DATA) public data: Moment,
    private service: PunchInService) {
    this.title = data.format('YYYY年MM月DD日签到');
  }

  ngOnInit() {
    this.loading = true
    this.service.getCost(this.data).subscribe(r => {
      this.loading = false;
      this.cost = r.cost;
      this.ticketCount = r.cost == 0 ? 0 : r.tickets;
      this.points = r.currentPoints;
    })
  }

  loading: boolean
  title: string
  cost = 0
  ticketCount = 0
  points = 0
}
