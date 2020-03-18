import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Environment, ENVIRONMENT } from 'environments/environment_token';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Moment } from 'moment';

export interface PunchInCost {
  currentPoints: number;
  cost: number;
  tickets: number;
}

export interface PunchInResult {
  success: boolean;
  consecutiveDays: number;
  points: number;
  expBonus: number;
  errorMessage?: string;
}

export interface PunchInHistory {
  timeStamp: string
  isMakeup: boolean
}

export interface PunchInHistoryResponse {
  punchIns: PunchInHistory[];
  legacySignDays?: number;
  minSignDate: Date;
}

function toDateString(d: Date): string {
  return `${d.getFullYear()}-${d.getMonth()+1}-${d.getDate()}`
}

@Injectable({
  providedIn: 'root'
})
export class PunchInService {

  host: string;

  constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
    this.host = env.apiHost;
  }

  getCost(date: Moment): Observable<PunchInCost> {
    return this.http.get<PunchInCost>(this.host + "/api/punchIn/cost", { params: { "date": date.format('YYYY-MM-DD') }, withCredentials: true });
  }

  punchIn(date?: Moment, useTicket?: boolean): Observable<PunchInResult> {
    let form = new FormData();
    if (date) {
      form.append("date", date.format('YYYY-MM-DD'));
    }
    if (useTicket) {
      form.append("useTicket", "true");
    }
    return this.http.post<PunchInResult>(this.host + "/api/punchIn/do", form, { withCredentials: true })
      .pipe(catchError((r: HttpErrorResponse) => {
        console.log(r);
        if (r.error.success === false) return of(r.error as PunchInResult);
        throw r;
      }));
  }

  history(y: number, m: number): Observable<PunchInHistoryResponse> {
    return this.http.get<PunchInHistoryResponse>(
      this.host + "/api/punchIn/history",
      { params: { year: y.toString(), month: m.toString() }, withCredentials: true })
  }
}
