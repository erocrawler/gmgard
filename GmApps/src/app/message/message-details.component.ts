import { EventEmitter } from '@angular/core';
import { Component, Input, OnInit, Output } from '@angular/core';
import { Observable } from 'rxjs';
import { MessageService } from './message.service';

@Component({
  selector: 'app-message-details',
  templateUrl: './message-details.component.html',
  styleUrls: ['./message-details.component.css']
})
export class MessageDetailsComponent implements OnInit {

  @Input()
  public id: number;

  @Input()
  public markRead: boolean = false;

  @Output()
  public delete = new EventEmitter<number>();

  loading = true;
  msg: MessageDetails;
  content: string;

  constructor(private service: MessageService) { }

  ngOnInit(): void {
    this.service.read(this.id, this.markRead).subscribe(
      m => {
        this.msg = m;
        this.loading = false;
        this.content = m.content.replace(/\n/g, "<br>")
      }
    );
  }

  handleDelete() {
    this.loading = true;
    this.service.delete(this.msg.messageId).subscribe(_ => {
      this.delete.emit(this.msg.messageId)
    });
  }
}
