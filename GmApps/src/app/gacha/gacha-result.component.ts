import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, OnDestroy } from "@angular/core";
import { GachaService } from "./gacha.service";
import { Router, ActivatedRoute } from "@angular/router";
import { MatDialog } from "@angular/material";
import { GachaResult, GachaItem } from "../models/GachaResult";
import { IGachaAnimation } from "./gacha-animation";
import { CardDetailComponent } from "./card-detail.component";
import { Observable } from "rxjs/Observable";
import { Subscription } from "rxjs/Subscription";
import "rxjs/add/observable/defer";
import "rxjs/add/observable/fromPromise";
import "rxjs/add/operator/finally";

@Component({
  selector: "app-gacha-result",
  templateUrl: "./gacha-result.component.html",
  styleUrls: ["./gacha-result.component.css"]
})
export class GachaResultComponent implements OnInit, AfterViewInit, OnDestroy {

  constructor(private router: Router,
    private route: ActivatedRoute,
    private gachaService: GachaService,
    private dialog: MatDialog) { }

    showType: string;
    count: number;
    result: GachaResult;
    animation: IGachaAnimation;
    @ViewChild("Canvas") canvasRef: ElementRef<HTMLCanvasElement>;
    @ViewChild("container") containerRef: ElementRef<HTMLDivElement>;
    cards: HTMLElement[];
    animationSub: Subscription;
    animationFinished = false;

    ngOnInit() {
        this.route.params.subscribe(q => {
          this.count = +q["count"];
          this.showType = q["showType"];
        });
        this.result = this.gachaService.getResult();
        if (!this.result || !this.result.items) {
            this.router.navigate(["/gacha"]);
        } else {
            this.cards = this.result.items.map(this.createCard);
            this.animation = this.gachaService.getAnimation(this.canvasRef.nativeElement as HTMLCanvasElement);
            if (!this.animation) {
                this.animationFinished = true;
                return;
            }
        }
        if (!this.cards) {
            return;
        }
        const container = this.containerRef.nativeElement as HTMLDivElement;
        const playlist = this.cards.reduce((obs, card) => obs.concat(Observable.defer(() => {
            container.appendChild(card);
            return this.animation.playGacha(card).then(_ => {
                container.removeChild(card);
                return true;
            });
        })), Observable.fromPromise(this.animation.playStart()));
        this.animationSub = playlist.finally(() => {
            this.animationFinished = true;
        }).subscribe(null, null, () => {
            console.log("finished");
        });
    }

    ngAfterViewInit() {

    }

    ngOnDestroy() {
        this.skipAnimation();
    }

    createCard(item: GachaItem): HTMLElement {
        const rarityMap = ["bronze", "bronze", "silver", "gold", "gold"];
        const frontside = document.createElement("div"), backside = document.createElement("div");
        frontside.className = "side";
        backside.className = "side back";
        if (navigator.userAgent.toLowerCase().indexOf("firefox") > -1) { //Firefox hack :(
            backside.style.zIndex = "100";
        }
        const cardFront = new Image();
        cardFront.src = "//static.gmgard.com/Images/gacha/" + item.name + ".png";
        const cardBack = new Image();
        cardBack.src = "//static.gmgard.com/Images/gacha/" + rarityMap[item.rarity - 1] + ".png";
        frontside.appendChild(cardFront);
        backside.appendChild(cardBack);
        const card = document.createElement("div");
        card.className = "card";
        card.appendChild(frontside);
        card.appendChild(backside);
        return card;
    }

    skipAnimation() {
        if (this.animation) {
            this.animation.stop();
            this.animationSub.unsubscribe();
            this.animationFinished = true;
        }
    }

    showDetails(name: string) {
        this.dialog.open(CardDetailComponent, { data: name });
    }

    cardUrl(name: string) {
        return this.gachaService.cardUrl(name);
    }
}
