import { Component, OnInit, Inject } from '@angular/core';
import { Environment, ENVIRONMENT } from 'environments/environment_token';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material';

interface RaffleConfig {
  isActive: boolean
  hasRaffle: boolean
  points: number
  cost: number
};

@Component({
  selector: 'app-raffle-index',
  templateUrl: './raffle-index.component.html',
  styleUrls: ['./raffle-index.component.css']
})
export class RaffleIndexComponent implements OnInit {

  constructor(public snackBar: MatSnackBar, private http: HttpClient, @Inject(ENVIRONMENT) private env: Environment) { }

  loading = true
  isActive = false
  hasRaffle = false
  points = 0
  cost = 0

  ngOnInit() {
    this.http.get<RaffleConfig>(this.env.apiHost + "/api/raffle", { withCredentials: true }).subscribe(r => {
      this.loading = false;
      this.isActive = r.isActive;
      this.hasRaffle = r.hasRaffle;
      this.points = r.points;
      this.cost = r.cost;
    });
  }

  buy() {
    this.loading = true;
    this.http.post(this.env.apiHost + "/api/raffle", '', { observe: "response", withCredentials: true }).subscribe(r => {
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
