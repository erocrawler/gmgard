import "rxjs/add/operator/catch";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import { Observable, of } from "rxjs";
import { Subject } from "rxjs/Subject";
import { Subscription } from "rxjs/Subscription";
import { Component, OnInit, OnChanges, Input } from "@angular/core";
import { MatSnackBar } from "@angular/material";

import { DLsite } from "../models/DLsite";
import { DlsiteSearchService } from "./dlsite-search.service";

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

    private searchSub: Subscription;
    private searchTitle = new Subject<string>();

    constructor(private dlsiteService: DlsiteSearchService, public snackBar: MatSnackBar) { }

    ngOnInit() {
        this.searchSub = this.searchTitle
            .debounceTime(500)
            .distinctUntilChanged()
            .switchMap(term => {
                if (term && term.length > 1) {
                    this.searchActive = true;
                    return this.dlsiteService.search(term);
                }
                return of([]);
            })
            .catch((err) => {
                this.searchActive = false;
                this.snackBar.open("DLsite搜索失败，请重试。", null, { duration: 3000 });
                return of<DLsite[]>([]);
            })
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
