import { Component, OnInit, Inject } from "@angular/core";
import { MatDialog, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { GachaService } from "./gacha.service";
import { GachaItemDetails } from "../models/GachaResult";

@Component({
  selector: "app-card-detail",
  templateUrl: "./card-detail.component.html",
  styleUrls: ["./card-detail.component.css"]
})
export class CardDetailComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: string, private gachaService: GachaService) { }

  loading = true;
  item: GachaItemDetails;
  url: string;

  private unknownItem: GachaItemDetails = {
    name: "unknown",
    title: "???",
    description: "??????",
    itemCount: 0,
    rarity: 0,
  }

  ngOnInit() {
    this.url = this.gachaService.cardUrl(this.data)
    this.gachaService.getDetails(this.data).subscribe(i => {
      this.loading = false;
      this.item = i || this.unknownItem;
    });
  }

}
