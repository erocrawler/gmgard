import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, OnDestroy, Renderer2 } from "@angular/core";
import { GachaService } from "./gacha.service";
import { Router, ActivatedRoute } from "@angular/router";
import { MatDialog } from "@angular/material/dialog";
import { GachaResult, GachaItem } from "../models/GachaResult";
import { IGachaAnimation, IGachaCard } from "./gacha-animation";
import { CardDetailComponent } from "./card-detail.component";
import { Subscription } from "rxjs/Subscription";
import { defer, from, concat } from "rxjs";
import { finalize } from "rxjs/operators";

class GachaCard implements IGachaCard {
  constructor(private self: HTMLElement, private container: HTMLElement, private renderer: Renderer2) { }

  private state: string
  setState(state: string): void {
    if (this.state == state) {
      return;
    }
    this.state = state;
    switch (this.state) {
      case "start":
        this.renderer.appendChild(this.container, this.self);
        this.renderer.addClass(this.self, "start");
        break;
      case "away":
        this.renderer.removeChild(this.self, "start");
        this.renderer.addClass(this.self, "away");
        break;
      case "end":
        this.renderer.removeChild(this.container, this.self);
        break;
    }
  }
  getState(): string {
    return this.state;
  }

}

@Component({
  selector: "app-gacha-result",
  templateUrl: "./gacha-result.component.html",
  styleUrls: ["./gacha-result.component.css"]
})
export class GachaResultComponent implements OnInit, AfterViewInit, OnDestroy {

  constructor(private router: Router,
    private route: ActivatedRoute,
    private gachaService: GachaService,
    private dialog: MatDialog,
    private renderer: Renderer2
  ) { }

  showType: string;
  count: number;
  result: GachaResult;
  animation: IGachaAnimation;
  @ViewChild("Canvas") canvasRef: ElementRef<HTMLCanvasElement>;
  @ViewChild("container") containerRef: ElementRef<HTMLDivElement>;
  cards: IGachaCard[];
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
    }
  }

  ngAfterViewInit() {
    if (!this.result) {
      return;
    }
    this.cards = this.result.items.map((i) => this.createCard(i));
    this.animation = this.gachaService.getAnimation(this.canvasRef.nativeElement as HTMLCanvasElement);
    if (!this.animation || !this.cards) {
      setTimeout(() => {
        this.animationFinished = true;
      }, 4)
      return;
    }
    setTimeout(() => {
      this.animationSub = from(this.animation.play(this.cards)).pipe(finalize(() => {
        this.animationFinished = true;
      })).subscribe();
    }, 4)
  }

  ngOnDestroy() {
    this.skipAnimation();
  }

  createCard(item: GachaItem): IGachaCard {
    const container = this.containerRef.nativeElement as HTMLDivElement;
    const rarityMap = ["bronze", "bronze", "silver", "gold", "gold"];
    const frontside = this.renderer.createElement("div"), backside = this.renderer.createElement("div");
    this.renderer.addClass(frontside, "side");
    this.renderer.addClass(backside, "side");
    this.renderer.addClass(backside, "back");
    if (navigator.userAgent.toLowerCase().indexOf("firefox") > -1) { //Firefox hack :(
      this.renderer.setStyle(backside, "zIndex", "100");
    }
    const cardFront = this.renderer.createElement('img');
    this.renderer.setProperty(cardFront, "src", "/assets/cards/" + item.name + ".png");
    const cardBack = this.renderer.createElement('img');
    this.renderer.setProperty(cardBack, "src",  "/assets/cards/" + rarityMap[item.rarity - 1] + ".png");
    this.renderer.appendChild(frontside, cardFront);
    this.renderer.appendChild(backside, cardBack);
    const card = this.renderer.createElement("div");
    this.renderer.addClass(card, "card");
    this.renderer.appendChild(card, frontside);
    this.renderer.appendChild(card, backside);
    return new GachaCard(card, container, this.renderer);
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
    return this.gachaService.cardCssUrl(name);
  }
}
