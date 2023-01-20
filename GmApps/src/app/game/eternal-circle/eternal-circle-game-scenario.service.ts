import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, Subject } from "rxjs";
import { GameScenario, GameStatus } from "../../models/GameScenario";
import { IGameScenarioProvider } from "../../shared/adv-game/scenario";
import { GameService, GameID } from "../game.service";

@Injectable({
  providedIn: 'root'
})
export class EternalCircleGameScenario implements IGameScenarioProvider {

  constructor(private gameService: GameService) {
  }

  status(): Observable<GameStatus> {
    return this.gameService.gameStatus(GameID.EternalCircle)
  }

  start(scene: GameScenario) {
    this.currentScene.next(scene);
  }

  restart(): Observable<GameStatus> {
    return this.gameService.gameRestart(GameID.EternalCircle);
  }

  currentScene = new BehaviorSubject<GameScenario>(null);
  next(choiceId: number) {
    this.gameService.nextScene(GameID.EternalCircle, choiceId).subscribe(r => this.currentScene.next(r));
  }
  prev() {
    this.gameService.prevScene(GameID.EternalCircle).subscribe(r => this.currentScene.next(r));
  }


}
