import { Component, OnInit, Inject } from "@angular/core";
import { Router, ActivatedRoute, Params } from "@angular/router";
import { MatButtonToggleChange } from "@angular/material/button-toggle";

import { Category } from "./category";
import { CategoryService } from "./category.service";
import { ENVIRONMENT } from "../../environments/environment_token";

@Component({
    templateUrl: "./title-helper.component.html",
    styleUrls: ["./title-helper.component.css"],
    providers: [CategoryService],
})
export class TitleHelperComponent implements OnInit {

    categories: Category[];
    selectedCategory: Category;
    title = "";

    dlsite = false;
    private host: string;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private service: CategoryService,
        @Inject(ENVIRONMENT) env: any) {
        this.host = window.location.protocol + env.apiHost;
        if (window.top !== window) {
            window.top.postMessage("gminit", this.host);
            window.addEventListener("message", this.handleMessage.bind(this));
        }
    }

    ngOnInit() {
        this.service.allCategories().then((cs: Category[]) => {
            this.categories = cs;
            this.route.params
                .subscribe((p: Params) => this.selectedCategory = this.categories.find((c: Category) => c.id === +p["id"]));
        });
    }

    setCategory(c: Category) {
        this.router.navigate(["title-helper", { id: c.id }]);
    }

    updateTitle(t) {
        this.title = t;
    }

    handleMessage(event: MessageEvent) {
        if (event.origin !== this.host) {
            return;
        }
        this.sendTitle();
    }

    sendTitle() {
        if (this.title && window.top !== window) {
            window.top.postMessage(this.title, this.host);
        }
    }

    toggleDlsite(event: MatButtonToggleChange) {
        this.dlsite = event.value;
    }
}
