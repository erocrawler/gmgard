import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TreasureHuntStatus, TreasureHuntAttemptResult } from 'app/models/TreasureHuntStatus';
import { GameScenario, GameStatus } from '../models/GameScenario';

export enum GameID {
  EternalCircle = 1,
  TarnishedWorld = 2
}

@Injectable({
  providedIn: 'root'
})
export class GameService {

  constructor(private http: HttpClient) {
  }

  treasureHuntStatus(): Observable<TreasureHuntStatus> {
    return this.http.get<TreasureHuntStatus>("/api/TreasureHunt/Status", { withCredentials: true });
  }

  treasureHuntAttempt(id: number, answer: string): Observable<TreasureHuntAttemptResult> {
    return this.http.post<TreasureHuntAttemptResult>("/api/TreasureHunt/Attempt", { id: id, answer: answer }, { withCredentials: true });
  }

  gameStatus(id: GameID): Observable<GameStatus> {
    return this.http.get<GameStatus>("/api/Game/Start", { withCredentials: true, params: { id: id } });
  }

  gameRestart(id: GameID): Observable<GameStatus> {
    return this.http.get<GameStatus>("/api/Game/Start", { withCredentials: true, params: { id: id, restart: true } });
  }

  gameChapter(id: GameID, chapter: number): Observable<GameStatus> {
    return this.http.get<GameStatus>("/api/Game/Start", { withCredentials: true, params: { id: id, jump: chapter } });
  }

  nextScene(id: GameID, p: number): Observable<GameScenario> {
    return this.http.post<GameScenario>("/api/Game/Next", null, { params: { progress: p, id: id },  withCredentials: true });
  }

  prevScene(id: GameID): Observable<GameScenario> {
    return this.http.post<GameScenario>("/api/Game/Prev", null, { params: { id: id }, withCredentials: true });
  }


  questionare(id: GameID, answers: number[]): Observable<GameScenario> {
    return this.http.post<GameScenario>("/api/Game/Questionare", answers, { params: { id: id }, withCredentials: true });
  }
}
