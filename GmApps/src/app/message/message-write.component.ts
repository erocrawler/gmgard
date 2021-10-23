import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { debounceTime, switchMap } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import { UserService } from '../auth/user.service';
import { UserSuggestion } from '../models/UserSuggestion';
import { MessageService } from './message.service';

@Component({
  selector: 'app-message-write',
  templateUrl: './message-write.component.html',
  styleUrls: ['./message-write.component.css']
})
export class MessageWriteComponent implements OnInit {

  action: string
  stateMsg: MessageDetails
  content: string
  title: string
  recipientControl: FormControl
  options: UserSuggestion[]
  sending = false;
  errors: string

  constructor(private router: Router, private auth: AuthService, private user: UserService,
    private service: MessageService, private snackBar: MatSnackBar) {
    const state = this.router.getCurrentNavigation()?.extras?.state;
    if (state && state.action) {
      this.action = state.action;
      this.stateMsg = state.msg;
    }
    this.recipientControl = new FormControl();
  }

  ngOnInit(): void {
    this.auth.getUser().subscribe(u => {
      if (this.action) {
        const pipe = new DatePipe("zh-CN");
        const parser = new DOMParser();
        this.content = "\n\n" + `---------${this.stateMsg.senderNickName}于${pipe.transform(this.stateMsg.sendDate, "short")}写道---------` + "\n" +
          parser.parseFromString(this.stateMsg.content, "text/html").documentElement.textContent;
      }
      if (this.action == "reply") {
        this.recipientControl.setValue(this.stateMsg.sender);
        if (u.userName === this.stateMsg.sender) {
          this.recipientControl.setValue(this.stateMsg.recipient);
        }
        this.title = "回复：" + (this.stateMsg.title || "");
      } else if (this.action == "forward") {
        this.title = "转发：" + (this.stateMsg.title || "");
      }
    });

    this.recipientControl.valueChanges.pipe(
      debounceTime(100),
      switchMap((v: string) => {
        if (v.length >= 2) {
          return this.user.suggestUser(this.recipientControl.value);
        }
        return [];
      })
    ).subscribe(s => {
      this.options = s;
    })
  }

  formatSuggestion(s: UserSuggestion) {
    if (s.nickName) {
      return `${s.userName} (${s.nickName})`;
    }
    return s.userName;
  }

  send() {
    this.errors = "";
    this.sending = true;
    this.service.send({ title: this.title, content: this.content, recipient: this.recipientControl.value }).subscribe(r => {
      this.sending = false;
      this.snackBar.open("发送成功", null, { duration: 3000 });
      this.router.navigate(["message", "outbox"])
    }, (err: HttpErrorResponse) => {
        if (err.error.errors) {
          this.errors = Object.values<string[]>(err.error.errors).map(cur => cur.join('；')).join('；');
          this.sending = false;
        }
    });
  }

}
