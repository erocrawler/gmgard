import { Observable, of } from "rxjs";
import "rxjs/add/operator/share";

import { Component, OnInit, Inject } from "@angular/core";
import { Router, ActivatedRoute, Params } from "@angular/router";
import { MatSnackBar } from "@angular/material/snack-bar";
import { DOCUMENT } from "@angular/common";
import { PageScrollService, PageScrollInstance } from "ngx-page-scroll-core";

import { BountyService } from "./bounty.service";
import { Paged } from "../models/Paged";
import { BountyPreview, BountyShowType } from "../models/Bounty";
import { first } from "rxjs/operators";

@Component({
  selector: "bounty-list",
  templateUrl: "./bounty-list.component.html",
  styleUrls: ["./bounty-list.component.css"]
})
export class BountyListComponent implements OnInit {

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private service: BountyService,
        private snackBar: MatSnackBar,
        private pageScrollService: PageScrollService,
        @Inject(DOCUMENT) private document: any) { }

    bounties: Paged<BountyPreview>;
    loading: boolean;
    showType: BountyShowType;

    private navSource = new Observable<Paged<BountyPreview>>();

    ngOnInit() {
        this.navSource = this.route.queryParams
            .switchMap((query: Params) => {
                const page = +query["page"] || 1;
                this.showType = query["show"] || "All";
                this.loading = true;
                return this.service.list(this.showType, page).catch(err => {
                    this.snackBar.open("列表加载失败，请刷新重试。", null, { duration: 3000 });
                    return of<Paged<BountyPreview>>();
                });
            }).share();
        this.navSource.subscribe((bounties: Paged<BountyPreview>) => {
                this.bounties = bounties;
                this.loading = false;
            });
    }

    navigate(page: number) {
        this.router.navigate(["bounty"], { queryParams: { "page": page }, queryParamsHandling: "merge" })
            .then((success: boolean) => {
                if (success) {
                    this.navSource.pipe(first()).subscribe(() =>
                        this.pageScrollService.start(
                          this.pageScrollService.create({ document: this.document, scrollTarget: "#bounty-list", duration: 500 })));
                }
            });
    }
}
