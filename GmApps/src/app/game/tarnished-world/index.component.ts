import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AlertComponent, AlertArg } from 'app/shared/alert-dialog';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { TarnishedWorldGameScenarioService } from './tarnished-world-game-scenario.service';
import { Dialog, GameScenario, GameStatus, Narrator } from 'app/models/GameScenario';
import { GameScenarios } from 'app/shared/adv-game/scenario';
import { ItemListComponent } from './item-list.component';

@Component({
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {

  constructor(private gameSceneService: TarnishedWorldGameScenarioService, public dialog: MatDialog) {
    this.scenarios = new GameScenarios(gameSceneService);
  }

  scenarios: GameScenarios
  loading = true
  status: GameStatus
  playing = false;
  isNewGame = true;

  ngOnInit() {
    this.loading = true;
    this.gameSceneService.status().subscribe(r => {
      this.updateStatus(r);
    })
  }

  private updateStatus(status: GameStatus) {
    this.status = status;
    this.isNewGame = this.status.newGameScenarioId === this.status.currentScenario.id;
    this.loading = false;
  }

  start() {
    this.playing = true;
    this.gameSceneService.start(this.status.currentScenario);
  }

  jump(id: number) {
    this.loading = true;
    this.gameSceneService.jump(id).subscribe(r => {
      this.updateStatus(r);
      this.playing = true;
      this.gameSceneService.start(this.status.currentScenario);
    });
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

  showInventory() {
    this.dialog.open(ItemListComponent, { data: this.status.inventory });
  }
}
