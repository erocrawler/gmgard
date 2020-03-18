import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Environment, ENVIRONMENT } from 'environments/environment_token';
import { Observable } from 'rxjs';
import { TreasureHuntStatus, TreasureHuntAttemptResult } from 'app/models/TreasureHuntStatus';
import { EternalCircleStatus, EternalCircleProgress } from 'app/models/EternalCircleStatus';

@Injectable({
  providedIn: 'root'
})
export class GameService {

  private host: string;

  constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
      this.host = env.apiHost;
  }

  treasureHuntStatus(): Observable<TreasureHuntStatus> {
    return this.http.get<TreasureHuntStatus>(this.host + "/api/TreasureHunt/Status", { withCredentials: true });
  }

  treasureHuntAttempt(id: number, answer: string): Observable<TreasureHuntAttemptResult> {
    return this.http.post<TreasureHuntAttemptResult>(this.host + "/api/TreasureHunt/Attempt", { id: id, answer: answer }, { withCredentials: true });
  }

  eternalCircleStatus(): Observable<EternalCircleStatus> {
    return this.http.get<EternalCircleStatus>(this.host + "/api/Game/EternalCircle", { withCredentials: true });
  }

  eternalCircleProgress(p: EternalCircleProgress): Observable<{}> {
    return this.http.post(this.host + "/api/Game/EternalCircle", null, { params: {"progress": p.toString() },  withCredentials: true, observe: "response" });
  }
}
