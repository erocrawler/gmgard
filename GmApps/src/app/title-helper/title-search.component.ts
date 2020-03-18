import "rxjs/add/operator/catch";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/switchMap";
import { Observable, Subject, of } from "rxjs";
import { Subscription } from "rxjs/Subscription";
import { Component, OnInit, OnChanges, Input } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";

import { BlogPreview } from "../models/Blog";
import { BlogSearchService } from "./blog-search.service";

@Component({
  selector: "title-search",
  templateUrl: "./title-search.component.html",
  styleUrls: ["./title-search.component.css"],
  providers: [BlogSearchService],
})
export class TitleSearchComponent implements OnInit, OnChanges {
    @Input()
    title: string;
    blogs: BlogPreview[];
    searchActive = false;
  
    private searchTitle = new Subject<string>();

    constructor(private blogService: BlogSearchService, public snackBar: MatSnackBar) { }

    ngOnInit() {
        this.searchTitle
            .debounceTime(300)
            .distinctUntilChanged()
            .switchMap(term => {
                if (term && term.length > 5) {
                    this.searchActive = true;
                    return this.blogService.search(this.title);
                }
                return of([]);
            })
            .catch((err) => {
                this.searchActive = false;
                this.snackBar.open("投稿搜索失败，请重试。", null, { duration: 3000 });
                return of<BlogPreview[]>([]);
            })
            .subscribe(b => {
                this.searchActive = false;
                this.blogs = b;
            })
    }

    ngOnChanges() {
        this.search();
    }

    search() {
        this.searchTitle.next(this.title);
    }
}
