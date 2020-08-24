import { Component, OnInit, Type } from "@angular/core";
import { Router, ActivatedRoute, NavigationEnd } from "@angular/router";

import { Toolbar } from "./toolbar/toolbar";
import { filter } from "rxjs/operators";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"]
})
export class AppComponent implements OnInit {
  title = "绅士应用";
  toolbar: Type<Toolbar>;

  constructor(private router: Router, private route: ActivatedRoute) { }

  ngOnInit() {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(_e => {
        const data = this.route.root.firstChild.snapshot.data;
        this.title = data["title"] || this.title;
        this.toolbar = data["toolbar"];
      });
  }
}
