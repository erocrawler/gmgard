import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, UrlSegment, NavigationEnd } from '@angular/router';
import { first } from 'rxjs/operators';
import { User } from '../models/User';
import { WheelService } from './wheel.service';

@Component({
  templateUrl: './wheel.component.html',
  styleUrls: ['./wheel.component.css']
})
export class WheelComponent implements OnInit {

  constructor(private router: Router, private route: ActivatedRoute, private service: WheelService) { }

  url: string
  isAdmin = false
  showA = false
  showB = false
  showC = false

  get firstWheel(): string {
    if (this.showA) {
      return 'a'
    } else if (this.showB) {
      return 'b'
    }
    return '';
  }

  updateUrl() {
    this.route.firstChild.url.pipe(first()).subscribe(d => {
      if (d.length) {
        this.url = d[0].path;
      } else {
        this.route.params.pipe(first()).subscribe(p => this.url = p['type'] || this.firstWheel);
      }
    });
  }

  ngOnInit() {
    this.router.events.filter(f => f instanceof NavigationEnd)
      .subscribe(_ => this.updateUrl());
    this.route.data.subscribe((d: { user: User }) => {
      this.isAdmin = d.user.isAdmanager()
    });
    this.service.getStatus().subscribe(s => {
      this.showA = s.wheelACost > 0;
      this.showB = s.wheelBCost > 0;
      this.showC = s.wheelCCost > 0;
      this.updateUrl();
    })
  }

}
