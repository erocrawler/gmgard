import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { GetInvitationCodeRequest, InvitationCodeResponse } from '../models/Invitations';
import { map, catchError } from 'rxjs/operators';
import { Observable, throwError } from 'rxjs';
import { RaffleConfig } from '../models/RaffleConfig';
import { Paged } from '../models/Paged';
import { IUser } from '../models/User';

export interface DraftResult {
  total: number,
  result: {
    user: IUser,
    code: string,
    date: Date,
  }[]
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  constructor(private http: HttpClient) {
  }

  getInvitationCode(request: GetInvitationCodeRequest): Observable<InvitationCodeResponse> {
    return this.http.post<InvitationCodeResponse>("/api/Admin/InvitationCode", request, { withCredentials: true })
      .pipe(catchError((result: HttpErrorResponse) => {
        if (result.status === 404) {
          return throwError({ message: "未找到" });
        } else if (result.error.error) {
          return throwError({ message: result.error.error })
        }
        return throwError({ message: result.message });
      }));
  }

  deleteInvitationCode(code: string, reason: string, notice: boolean): Observable<boolean> {
    return this.http.delete("/api/Admin/InvitationCode",
      {
        params: {
          code: code, reason: reason, notice: String(notice)
        },
        withCredentials: true,
        observe: "response"
      })
      .pipe(catchError((result: HttpErrorResponse) => {
        if (result.status === 404) {
          return throwError({ message: "未找到" });
        }
        else if (result.status == 400) {
          return throwError({
            message: "无效的邀请码:" + (result.error["error"] || "")
          });
        }
        return throwError({ message: result.message });
      }), map(r => r.ok));
  }

  allRaffles(page: number): Observable<Paged<RaffleConfig>> {
    return this.http.get<Paged<RaffleConfig>>("/api/Raffle/All", { params: { page }})
  }

  addRaffle(config: RaffleConfig): Observable<boolean> {
    return this.http.put("/api/Raffle", config, { observe: "response" }).pipe(map(r => r.ok))
  }

  updateRaffle(config: RaffleConfig): Observable<boolean> {
    return this.http.patch("/api/Raffle", config, { observe: "response" }).pipe(map(r => r.ok))
  }

  draftRaffle(id: number): Observable<DraftResult> {
    return this.http.get<DraftResult>("/api/Raffle/Draft", { params: { id } });
  }
}
