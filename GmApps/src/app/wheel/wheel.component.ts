import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, UrlSegment, NavigationEnd } from '@angular/router';
import { first } from 'rxjs/operators';
import { User } from '../models/User';

@Component({
  templateUrl: './wheel.component.html',
  styleUrls: ['./wheel.component.css']
})
export class WheelComponent implements OnInit {

  constructor(private router: Router, private route: ActivatedRoute) { }

  url: string
  isAdmin = false

  updateUrl() {
    this.route.firstChild.url.pipe(first()).subscribe(d => {
      if (d.length) {
        this.url = d[0].path;
      } else {
        this.route.params.pipe(first()).subscribe(p => this.url = p['type'] || "a");
      }
    });
  }

  ngOnInit() {
    this.updateUrl();
    this.router.events.filter(f => f instanceof NavigationEnd)
      .subscribe(_ => this.updateUrl());
    this.route.data.subscribe((d: { user: User }) => {
      this.isAdmin = d.user.isAdmanager()
    });
  }

}
