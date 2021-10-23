import { Component, OnInit } from '@angular/core';
import { Toolbar } from './toolbar';

@Component({
  selector: 'app-message-toolbar',
  templateUrl: './message-toolbar.component.html',
})
export class MessageToolbarComponent extends Toolbar implements OnInit {

  constructor() {
      super();
  }

  ngOnInit(): void {
  }

}
