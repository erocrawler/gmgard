import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Params } from '@angular/router';
import { RaffleConfig } from '../models/RaffleConfig';

@Component({
  selector: 'app-raffle-index',
  templateUrl: './raffle-index.component.html',
  styleUrls: ['./raffle-index.component.css']
})
export class RaffleIndexComponent implements OnInit {

  constructor(public snackBar: MatSnackBar, private route: ActivatedRoute, private http: HttpClient) { }

  startTime: Date
  endTime: Date
  loading = true
  isActive = false
  hasRaffle = false
  points = 0
  cost = 0
  img = ""
  title = ""
  endMsg = ""
  raffleId = 0

  ngOnInit() {
    this.route.params.subscribe((p: Params) => {
      this.raffleId = +p["id"];
      this.http.get<RaffleConfig>("/api/raffle", { params: { id: this.raffleId }, withCredentials: true }).subscribe(r => {
        this.loading = false;
        this.title = r.title
        this.startTime = r.eventStart;
        this.endTime = r.eventEnd;
        this.isActive = r.isActive;
        this.hasRaffle = r.hasRaffle;
        this.points = r.points;
        this.cost = r.raffleCost;
        this.img = r.image;
        this.endMsg = (new Date() >= new Date(this.endTime)) ? "抽奖已结束！抽奖结果请稍后参阅公告。" : "抽奖即将开始，敬请期待！";
      }, (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 404) {
          this.title = "不存在的"
          this.endMsg = "不存在此抽奖！"
        } else {
          this.snackBar.open("读取出错，请刷新重试", null, { duration: 3000 });
        }
      });
    })
  }

  buy() {
    this.loading = true;
    this.http.post("/api/raffle", '', { params: { id: this.raffleId }, observe: "response", withCredentials: true }).subscribe(r => {
      this.loading = false;
      if (r.ok) {
        this.snackBar.open("彩券购买成功", null, { duration: 3000 });
        this.hasRaffle = true;
        this.points -= this.cost;
      } else {
        this.snackBar.open("购买出错，请刷新重试", null, { duration: 3000 });
      }
    })
  }
}
