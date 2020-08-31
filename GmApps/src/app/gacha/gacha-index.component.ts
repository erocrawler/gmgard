import { Component, OnInit, Inject } from "@angular/core";
import { User } from "../models/User";
import { AuthService } from "../auth/auth.service";
import { GachaService } from "./gacha.service";
import { MatDialog, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router, ActivatedRoute, Params } from "@angular/router";
import { GachaPool } from "app/gacha/gacha-pools";
import { combineLatest, race, first, switchMap } from "rxjs/operators";
import { fromEvent, forkJoin } from "rxjs";

interface GachaPreference {
  showAnimation: boolean
  quality: "low" | "high"
  introViewed?: string[]
}

@Component({
  selector: "gacha-index",
  templateUrl: "./gacha-index.component.html",
  styleUrls: ["./gacha-index.component.css"]
})
export class GachaIndexComponent implements OnInit {

  constructor(
    private authService: AuthService,
    private gachaService: GachaService,
    private router: Router,
    private route: ActivatedRoute,
    public dialog: MatDialog,
    public snackBar: MatSnackBar) {
    this.supportAnimation = this.gachaService.supportAnimation();
    this.supportVideoPhone = !this.isTouchDevice() || "playsInline" in document.createElement("video");
  }

  pool: GachaPool;
  currentUser: User;
  quality_: "low" | "high" = "low";
  introViewed_: string[] = [];
  showAnimation_ = false;
  loadActive = false;
  progressMode: "query" | "determinate" = "query";
  progressValue = 0;
  supportAnimation = false;
  supportVideoPhone = false;

  ngOnInit() {
    this.authService.getUser().subscribe(u => {
      this.currentUser = u;
    });
    this.showAnimation_ = this.gachaService.supportAnimation();
    this.quality_ = this.supportVideoPhone ? "high" : "low";
    this.loadDefaults();
    this.route.params.subscribe(p => {
      const c = Number(p["count"]);
      if (c === 1 || c === 10) {
        setTimeout(() => this.confirm(c));
      }
    });
    this.route.params.pipe(
      combineLatest(this.gachaService.getPools())
    ).subscribe(([d, pools]: [Params, Map<string, GachaPool>]) => {
      let showType: string = d["showType"];
      if (!pools.has(showType)) {
        showType = pools.keys().next().value;
      }
      this.pool = pools.get(showType);
      if (this.pool.hasIntro && !this.introViewed_.includes(showType)) {
        this.introViewed_.push(showType);
        this.setDefaults();
        this.router.navigate(["/gacha", "intro", {showType: showType}])
      }
    });
  }

  get quality() { return this.quality_ }

  set quality(val: "low" | "high") {
    this.quality_ = val;
    this.setDefaults();
  }

  get showAnimation() { return this.showAnimation_; }

  set showAnimation(val: boolean) {
    this.showAnimation_ = val;
    this.setDefaults();
  }

  setDefaults() {
    const obj: GachaPreference = {
      showAnimation: this.showAnimation_,
      quality: this.quality_,
      introViewed: this.introViewed_,
    }
    localStorage.setItem("gmGachaSettings", JSON.stringify(obj));
  }

  loadDefaults() {
    const item = localStorage.getItem("gmGachaSettings");
    if (item) {
      const obj = JSON.parse(item) as GachaPreference;
      this.showAnimation_ = obj.showAnimation;
      this.quality_ = obj.quality;
      this.introViewed_ = obj.introViewed || [];
    }
  }

  confirm(count: 1 | 10) {
    const dialogRef = this.dialog.open(GachaConfirmComponent, { data: count });
    let type: "none" | "low" | "high" = this.quality;
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadActive = true;
        const doGacha = () => {
          this.progressMode = "query";
          this.gachaService.gacha(this.pool.poolName, count)
            .pipe(switchMap(r => {
              if (!r.success) {
                throw new Error(r.errorMessage);
              }
              const obs = r.items.map(i => {
                const img = new Image();
                const o = fromEvent(img, "load").pipe(race(fromEvent(img, "error")), first());
                img.src = "/assets/cards/" + i.name + ".png";
                return o;
              });
              return forkJoin(...obs)
            }))
            .subscribe(_ => {
              this.router.navigate(["/gacha", "result", { count, showType: this.pool.poolName }])
            },
              (error: Error) => {
                this.snackBar.open(error.message || "加载失败，请刷新重试。", null, { duration: 3000 });
                this.loadActive = false;
              });
        };
        if (!this.showAnimation) {
          type = "none";
        }
        this.gachaService.loadAnimation(type).subscribe(
          (next: number) => {
            if (next > 0) {
              this.progressMode = "determinate";
              this.progressValue = next;
            } else {
              this.progressMode = "query";
            }
          },
          (error: any) => {
            this.snackBar.open("动画加载失败，请重试。", null, { duration: 3000 });
            this.loadActive = false;
          },
          doGacha
        )
      }
    })
  }

  private isTouchDevice(): boolean {
    return "ontouchstart" in window        // works on most browsers
      || !!navigator.maxTouchPoints;       // works on IE10/11 and Surface
  }
}

@Component({
  selector: "gacha-confirm",
  templateUrl: "./gacha-confirm.component.html",
})
export class GachaConfirmComponent implements OnInit {
  constructor(@Inject(MAT_DIALOG_DATA) public data: 1 | 10, private authService: AuthService) { }

  cost = 10;
  points = 0;

  ngOnInit() {
    this.authService.getUser(true).subscribe(u => this.points = u.points);
  }

}
