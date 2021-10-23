import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TreasureHuntStatus, TreasureHuntAttemptResult } from 'app/models/TreasureHuntStatus';
import { EternalCircleStatus, EternalCircleProgress } from 'app/models/EternalCircleStatus';

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

  eternalCircleStatus(): Observable<EternalCircleStatus> {
    return this.http.get<EternalCircleStatus>("/api/Game/EternalCircle", { withCredentials: true });
  }

  eternalCircleProgress(p: EternalCircleProgress): Observable<{}> {
    return this.http.post("/api/Game/EternalCircle", null, { params: {"progress": p.toString() },  withCredentials: true, observe: "response" });
  }
}
