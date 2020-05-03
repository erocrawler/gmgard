import { Component, OnInit, ViewChild, ElementRef } from "@angular/core";
import gsap from "gsap";
import { WheelService } from "./wheel.service";
import { ActivatedRoute } from "@angular/router";
import { map, zip, combineLatest } from "rxjs/operators";
import { PrizeInfo, SpinWheelStatus, VoucherKind, LuckyPointVoucher, SpinWheelResult, IVoucher } from "../models/Vouchers";
import * as moment from "moment";
import { MatDialog } from "@angular/material/dialog";
import { SpinConfirmComponent, SpinConfirmArg } from "./dialogs/spin-confirm.component";
import { MatSnackBar } from "@angular/material/snack-bar";
import { PrizeConfirmComponent } from "./dialogs/prize-confirm.component";
import { RedeemCeilingComponent } from "./dialogs/redeem-ceiling.component";
import { RedeemPointsComponent } from "./dialogs/redeem-points.component";
import { of, concat  } from "rxjs";

var Winwheel: WinWheel = require("winwheel");

@Component({
  templateUrl: './wheel-index.component.html',
  styleUrls: ['./wheel-index.component.css']
})
export class WheelIndexComponent implements OnInit {
  loading = false;
  title = "";
  isActive = false;
  spinning = false;
  wheelType = "a";
  wheel: WinWheel;
  status: SpinWheelStatus;
  userPoints: number;
  luckyPoints: number;
  spentPoints: number;
  ceilingCost: number;
  ceilingCount: number;
  ceilingProgress: number;

  constructor(private wheelService: WheelService, private route: ActivatedRoute,
    public dialog: MatDialog, public snackBar: MatSnackBar) {
  }

  ngOnInit() {
    this.wheelService.getStatus().subscribe(status => {
      this.updateStatus(status)
      this.route.params.pipe(
        map(p => p["type"] == "b" ? "b" : "a"),
      ).subscribe((wheelType) => {
        this.wheelType = wheelType
        if (wheelType == 'a') {
          this.newWheel(this.status.wheelAPrizes)
        } else {
          this.newWheel(this.status.wheelBPrizes)
        }
      })

    })
    this.loading = true;
  }

  private updateStatus(status: SpinWheelStatus) {
    this.loading = false;
    this.title = status.title;
    this.isActive = status.isActive;
    this.status = status;
    this.userPoints = status.userPoints;
    let luckyVouchers = status.vouchers.filter(v => v instanceof LuckyPointVoucher);
    this.luckyPoints = luckyVouchers.reduce((total, l: LuckyPointVoucher) => total + l.currentValue, 0);
    this.spentPoints = luckyVouchers.reduce((total, l: LuckyPointVoucher) => total + l.usedValue, 0);
    this.ceilingCount = status.vouchers.filter(v => v.kind == VoucherKind.CeilingPrize).length;
    let val = (this.spentPoints - this.ceilingCount * this.status.ceilingCost) / this.status.ceilingCost;
    this.ceilingProgress = (val > 1 ? 1 : val) * 100;
  }

  private newWheel(prizes: PrizeInfo[]) {
    let getStyle = (p: PrizeInfo) => {
      if (p.isRealItem) {
        return p.drawPercentage < 10 ? '#ff0000' : '#fe7676';
      } else if (p.isVoucher) {
        return '#f69562';
      }
      return '#fff7e2';
    }
    let totalPercent = prizes.reduce((total, p) => p.drawPercentage + total, 0);
    let config = {
      'canvasId': 'spinwheel',
      'outerRadius': 150,
      'textFontSize': 18,
      'innerRadius': 28,
      'textOrientation': 'vertical',
      'textAlignment': 'outer',
      'numSegments': 12,
      'segments': prizes.map(p => {
        return {
          'fillStyle': getStyle(p),
          'text': p.prizeName,
          'size': (p.drawPercentage / totalPercent)*360
        }
      }),
      'rotationAngle': 0,
    };
    this.wheel = new Winwheel(config);
    this.wheel.draw(true);
  }

  confirmRedeemPoints() {
    this.dialog.open<RedeemPointsComponent, void, boolean>(RedeemPointsComponent)
      .beforeClosed().subscribe(refresh => {
      if (!refresh) {
        return
      }
      this.wheelService.getStatus().subscribe(s => this.updateStatus(s));
    })
  }

  get canRedeemCeiling(): boolean {
    return (this.ceilingCount + 1) * this.status.ceilingCost <= this.spentPoints;
  }

  confirmRedeemCeiling() {
    this.loading = true
    this.wheelService.redeemCeiling().subscribe(voucher => {
      this.loading = false
      this.dialog.open<RedeemCeilingComponent, IVoucher>(RedeemCeilingComponent, { data: voucher });
    }, err => {
      this.loading = false;
      this.snackBar.open("兑换失败，请刷新重试。", null, { duration: 3000 });
    })
  }

  get bjTimeNow(): moment.Moment {
    return moment().utcOffset(480)
  }

  get canSpin(): boolean {
    if (!this.isActive || this.loading || this.spinning) {
      return false;
    }
    return true;
  }

  confirmSpin() {
    let arg: SpinConfirmArg;
    let prizes: PrizeInfo[] = [];
    if (this.wheelType === 'a') {
      let todaySpin = this.status.vouchers.filter(v => v.kind == VoucherKind.WheelA
        && moment(v.useTime).isSame(this.bjTimeNow, 'day'));
      arg = {
        isLimit: todaySpin.length >= 3,
        points: concat(of(this.userPoints), this.wheelService.getStatus().pipe(map(s => {
          this.updateStatus(s);
          return this.userPoints;
        }))),
        wheelCost: this.status.wheelACost,
        wheelType: 'a'
      }
      prizes = this.status.wheelAPrizes
    } else {
      arg = {
        isLimit: false,
        points: concat(of(this.luckyPoints), this.wheelService.getStatus().pipe(map(s => {
          this.updateStatus(s);
          return this.luckyPoints;
        }))),
        wheelCost: this.status.wheelBCost,
        wheelType: 'b'
      }
      prizes = this.status.wheelBPrizes
    }
    let diag = this.dialog.open<SpinConfirmComponent, SpinConfirmArg, boolean>(SpinConfirmComponent, { data: arg });
    diag.beforeClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }
      this.loading = true;
      this.wheelService.spin(this.wheelType).subscribe(r => {
        this.loading = false;
        let indexes: number[] = [];
        let prizeName = r.prize.prizeName;
        if (prizeName.endsWith("（售罄）")) {
          prizeName = prizeName.substring(0, prizeName.length - 4)
        }
        for (let i = 0; i < prizes.length; i++) {
          if (prizes[i].prizeName === r.prize.prizeName) {
            indexes.push(i)
          }
        }
        this.spin(indexes[Math.floor(Math.random() * indexes.length)] + 1, r);
        this.wheelService.getStatus().subscribe(s => this.updateStatus(s));
      }, err => {
          this.loading = false;
          this.snackBar.open("转盘失败，请刷新重试。", null, { duration: 3000 });
        })
    })
  }

  spin(idx: number, prize: SpinWheelResult) {
    this.spinning = true;
    let onComplete = () => {
      this.spinning = false;
      this.dialog.open<PrizeConfirmComponent, SpinWheelResult>(PrizeConfirmComponent, { data: prize });
    };

    let winwheelAnimationLoop = () => {
      let self = this.wheel;

      if (self) {
        if (self.animation.clearTheCanvas) {
          self.ctx.clearRect(0, 0, self.canvas.width, self.canvas.height);
        }
        self.draw(false);
      }
    };
    this.wheel.rotationAngle = 0;
    this.wheel.animation = {
      'type': 'spinToStop',
      'duration': 10, // Duration in seconds.
      'spins': 3, // Default number of complete spins.
      'stopAngle': this.wheel.getRandomForSegment(idx),
    },
    this.wheel.computeAnimation();

    let animation = this.wheel.animation,
      properties: gsap.TweenVars = {
        yoyo: animation.yoyo,
        repeat: animation.repeat,
        ease: animation.easing,
        onUpdate: winwheelAnimationLoop,
        onComplete: onComplete
      };
    properties[animation.propertyName] = animation.propertyValue;
    this.wheel.tween = gsap.to(this.wheel, animation.duration, properties);
  }
}
