import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserSuggestion } from '../models/UserSuggestion';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) { }

  suggestUser(name: string): Observable<UserSuggestion[]> {
    return this.http.post<UserSuggestion[]>("/api/Account/SuggestUser", null, { params: { name }, withCredentials: true });
  }
}
