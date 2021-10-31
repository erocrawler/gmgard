import { animate, state, style, transition, trigger } from '@angular/animations';
import { ElementRef, EventEmitter, HostBinding, OnChanges } from '@angular/core';
import { Component, Input, OnInit, Output } from '@angular/core';
import { MessageService } from './message.service';

@Component({
  selector: 'app-message-details',
  templateUrl: './message-details.component.html',
  styleUrls: ['./message-details.component.css'],
  animations: [
    trigger('grow', [
      transition('void <=> *', []),
      transition('* <=> *', [
        style({height: '{{startHeight}}px', opacity: 0}),
        animate('.5s ease'),
      ], {params: {startHeight: 0}})
    ]),
  ]
})
export class MessageDetailsComponent implements OnInit {

  @Input()
  public id: number;

  @Input()
  public markRead: boolean = false;

  @Output()
  public delete = new EventEmitter<number>();

  @HostBinding('@grow') grow: any;

  loading = true;
  msg: MessageDetails;
  content: string;
  startHeight: number;

  constructor(private service: MessageService, private element: ElementRef) { }

  ngOnInit(): void {
    this.service.read(this.id, this.markRead).subscribe(
      m => {
        this.msg = m;
        this.loading = false;
        this.content = m.content.replace(/\n/g, "<br>")

        this.startHeight = this.element.nativeElement.clientHeight;

        this.grow = {
          value: this.loading,
          params: { startHeight: this.startHeight }
        };
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
