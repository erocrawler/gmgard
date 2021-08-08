import { Component, OnInit, Type } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from "@angular/router";

import { Toolbar } from "./toolbar/toolbar";
import { filter } from "rxjs/operators";

@Component({
  selector: 'app-app-layout',
  templateUrl: './app-layout.component.html',
  styleUrls: ['./app-layout.component.css']
})
export class AppLayoutComponent implements OnInit {

  title = "绅士应用";
  toolbar: Type<Toolbar>;

  constructor(private router: Router, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.firstChild.data.subscribe(data => {
      this.title = data["title"] || this.title;
      this.toolbar = data["toolbar"];
    })
  }

}
