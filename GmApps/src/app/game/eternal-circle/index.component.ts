import { Component, OnInit } from '@angular/core';
import { GameService } from '../game.service';
import { EternalCircleStatus, EternalCircleProgress } from 'app/models/EternalCircleStatus';
import { MatDialog } from '@angular/material';
import { AlertComponent, AlertArg } from 'app/shared/alert-dialog';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {

  constructor(private gameService: GameService, private router: Router, public dialog: MatDialog) { }

  loading = true
  status: EternalCircleStatus

  ngOnInit() {
    this.gameService.eternalCircleStatus().subscribe(r => {
      this.status = r
      this.loading = false;
    })
  }

  confirmRetry() {
    this.dialog.open<AlertComponent, AlertArg, boolean>(AlertComponent,
      { data: { title: "重新开始", message: "确认要重新开始游戏吗？" } })
      .afterClosed().pipe(
        switchMap(ok => {
          if (ok) {
            return this.gameService.eternalCircleProgress(EternalCircleProgress.Prologue);
          }
          return of(false);
        })
      ).subscribe(ok => {
        if (ok) {
          this.router.navigate(['/game/eternal-circle', 'play']);
        }
      })
  }
}
