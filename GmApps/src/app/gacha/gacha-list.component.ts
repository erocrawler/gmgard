import { Component, OnInit } from "@angular/core";
import { MatDialog } from "@angular/material";
import { CardDetailComponent } from "./card-detail.component";
import { GachaService } from "./gacha.service";
import { GachaStats, GachaItemDetails } from "app/models/GachaResult";
import { PageEvent } from '@angular/material/paginator';

@Component({
  selector: "app-gacha-list",
  templateUrl: "./gacha-list.component.html",
  styleUrls: ["./gacha-list.component.css"]
})
export class GachaListComponent implements OnInit {

    constructor(private gachaService: GachaService, private dialog: MatDialog) { }

    stats: GachaStats;
    pagedCards: GachaItemDetails[];

    ngOnInit() {
        this.gachaService.getStats().subscribe(s => {
            this.stats = s;
            this.pagedCards = s.userCards.slice(0, 20);
        });
    }

    showDetails(name: string) {
        this.dialog.open(CardDetailComponent, { data: name });
    }

    cardUrl(name: string) {
        return this.gachaService.cardUrl(name);
    }

    updatePage(ev: PageEvent) {
        this.pagedCards = this.stats.userCards.slice(ev.pageIndex * ev.pageSize, (ev.pageIndex + 1) * ev.pageSize);
    }
}
