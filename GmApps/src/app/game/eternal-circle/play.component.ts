import { Component, OnInit, ViewChild } from '@angular/core';
import { Scenarios } from './scenario';
import { EternalCircleStatus, EternalCircleProgress } from 'app/models/EternalCircleStatus';
import { GameService } from '../game.service';
import { GameScenarios } from '../../shared/adv-game/scenario';
import { AdvGameComponent } from '../../shared/adv-game/adv-game.component';
import { Router } from '@angular/router';

@Component({
  templateUrl: './play.component.html',
  styleUrls: ['./play.component.css'],
})
export class PlayComponent implements OnInit {

  constructor(private gameService: GameService, private router: Router) { }

  status: EternalCircleStatus;
  scenarios = new GameScenarios<EternalCircleProgress>(Scenarios);
  @ViewChild(AdvGameComponent) game: AdvGameComponent<EternalCircleProgress>;

  ngOnInit() {
    this.gameService.eternalCircleStatus().subscribe(r => {
      this.status = r;
      this.game.playChapter(r.progress);
    })
  }

  handleChoice(choice: EternalCircleProgress) {
    if (this.game.isEnded) {
      this.status.retryCount++;
    }
    if (this.status.retryCount > 0 && choice == EternalCircleProgress.ActEX) {
      choice = EternalCircleProgress.Act5;
    }
    this.gameService.eternalCircleProgress(choice).subscribe(_ => {
      this.game.playChapter(choice);
    })
  }

  handleExit() {
    this.router.navigate(['/game/eternal-circle']);
  }
}
