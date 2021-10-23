import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Paged } from '../models/Paged';
import { MessageService } from './message.service';

@Component({
  selector: 'app-message-inbox',
  templateUrl: './message-inbox.component.html',
  styleUrls: ['./message-inbox.component.css']
})
export class MessageInboxComponent implements OnInit {

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private service: MessageService) { }

  loading = false;
  unreadOnly = false;
  messages: Paged<MessageDisplay>;
  page: number;

  ngOnInit(): void {
    this.route.params
      .pipe(
        switchMap((query: Params) => {
          this.page = +query["page"] || 1;
          this.loading = true;
          return this.service.inbox(this.page, this.unreadOnly).pipe(
            catchError(() => {
              this.snackBar.open("列表加载失败，请刷新重试。", null, { duration: 3000 });
              return of<Paged<MessageDisplay>>();
            })
          );
        })
    ).subscribe(md => {
      this.loading = false;
      this.messages = md;
    })
  }

  handleOpen(msg: MessageDisplay) {
    msg.isRead = true;
  }

  navigate(num: number) {
    this.router.navigate([{ page: num }], { relativeTo: this.route });
  }

  refresh() {
    this.service.outbox(this.page).pipe(
      catchError(() => {
        this.snackBar.open("列表加载失败，请刷新重试。", null, { duration: 3000 });
        return of<Paged<MessageDisplay>>();
      })
    ).subscribe(md => this.messages = md);
  }
}
