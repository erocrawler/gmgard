import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Paged } from '../models/Paged';
import { SendMessageRequest } from '../models/SendMessageRequest';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(private http: HttpClient) {
  }

  inbox(page: number = 1, unreadOnly: boolean = false): Observable<Paged<MessageDisplay>> {
    return this.http.get<Paged<MessageDisplay>>("/api/Message/Inbox", { params: { pagenum: String(page), unreadOnly: String(unreadOnly) }, withCredentials: true });
  }

  read(id: number, markRead: boolean = true): Observable<MessageDetails> {
    return this.http.get<MessageDetails>("/api/Message/Content", { params: { id: String(id), markRead: String(markRead) }, withCredentials: true });
  }

  outbox(page: number = 1): Observable<Paged<MessageDisplay>> {
    return this.http.get<Paged<MessageDisplay>>("/api/Message/Outbox", { params: { pagenum: String(page) }, withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>("/api/Message/Delete", { params: { id: String(id) }, withCredentials: true });
  }

  send(message: SendMessageRequest): Observable<boolean> {
    return this.http.post("/api/Message/Send", message, { withCredentials: true, observe: "response" }).pipe(
      map(r => r.ok)
    );
  }
}
