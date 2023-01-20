import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AlertComponent, AlertArg } from 'app/shared/alert-dialog';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { EternalCircleGameScenario } from './eternal-circle-game-scenario.service';
import { Dialog, GameScenario, GameStatus, Narrator } from '../../models/GameScenario';
import { Scenarios } from './scenario';
import { GameScenarios } from '../../shared/adv-game/scenario';

@Component({
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {

  constructor(private gameSceneService: EternalCircleGameScenario, public dialog: MatDialog) {
    this.seedScene = Scenarios.map(s => {
      let gs: GameScenario = {
        id: s.title,
        dialogs: s.dialogs.map(d => {
          let ds: Dialog = {
            bgImg: d.bgImg,
            effect: d.effect,
            texts: d.texts
          };
          return ds;
        }),
        narrators: s.narrators.map(n => {
          let ns: Narrator = {
            avatar: n.avatar,
            name: n.name,
            display: n.display || n.name,
          }
          return ns;
        }),
        next: s.next || [],
      };
      return gs;
    });
    this.scenarios = new GameScenarios(gameSceneService);
  }

  scenarios: GameScenarios
  loading = true
  status: GameStatus
  playing = false;
  seedScene: GameScenario[]
  isNewGame = true;

  ngOnInit() {
    this.loading = true;
    this.gameSceneService.status().subscribe(r => {
      this.status = r
      this.isNewGame = this.status.newGameScenarioId === this.status.currentScenario.id;
      this.loading = false;
    })
  }

  start() {
    this.playing = true;
    this.gameSceneService.start(this.status.currentScenario);
  }

  confirmRetry() {
    this.dialog.open<AlertComponent, AlertArg, boolean>(AlertComponent,
      { data: { title: "重新开始", message: "确认要重新开始游戏吗？" } })
      .afterClosed().pipe(
        switchMap(ok => {
          if (ok) {
            return this.gameSceneService.restart();
          }
          return of(null);
        })
      ).subscribe(status => {
        if (status) {
          this.status = status;
          this.isNewGame = true;
          this.playing = true;
          this.gameSceneService.start(this.status.currentScenario);
        }
      })
  }

  handleExit() {
    this.playing = false;
    this.ngOnInit();
  }

  stringify(o: Object): string {
    return JSON.stringify(o);
  }
}
