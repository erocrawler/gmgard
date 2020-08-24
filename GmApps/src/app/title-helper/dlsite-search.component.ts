import { of, Subject } from "rxjs";
import { Component, OnInit, OnChanges, Input } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";

import { DLsite } from "../models/DLsite";
import { DlsiteSearchService } from "./dlsite-search.service";
import { debounceTime, distinctUntilChanged, switchMap, catchError } from "rxjs/operators";

@Component({
  selector: "dlsite-search",
  templateUrl: "./dlsite-search.component.html",
  styleUrls: ["./dlsite-search.component.css"],
  providers: [DlsiteSearchService]
})
export class DlsiteSearchComponent implements OnInit, OnChanges {
  @Input()
  title: string;
  entries: DLsite[];
  searchActive = false;
  notFound = false;
  private searchTitle = new Subject<string>();

  constructor(private dlsiteService: DlsiteSearchService, public snackBar: MatSnackBar) { }

  ngOnInit() {
    this.searchTitle.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(term => {
        if (term && term.length > 1) {
          this.searchActive = true;
          return this.dlsiteService.search(term);
        }
        return of([]);
      }),
      catchError(_ => {
        this.searchActive = false;
        this.snackBar.open("DLsite搜索失败，请重试。", null, { duration: 3000 });
        return of<DLsite[]>([]);
      }))
      .subscribe(b => {
        this.searchActive = false;
        this.notFound = this.title.length > 1 && b.length === 0;
        this.entries = b;
      });
    this.search();
  }

  ngOnChanges() {
    this.search();
  }

  search() {
    this.notFound = false;
    this.searchTitle.next(this.title);
  }
}
