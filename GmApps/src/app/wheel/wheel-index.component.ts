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
import { of, concat, Subject, Observable  } from "rxjs";
import { User } from "../models/User";
import { RedeemCouponComponent } from "./dialogs/redeem-coupon.component";

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
  user: User;
  userPoints: number;
  luckyPoints: number;
  totalPoints: number;
  ceilingCost: number;
  ceilingCount: number;
  ceilingProgress: number;
  spinLimit: number;
  display1: PrizeInfo[];
  display2: PrizeInfo[];

  constructor(private wheelService: WheelService, private route: ActivatedRoute,
    public dialog: MatDialog, public snackBar: MatSnackBar) {
  }

  ngOnInit() {
    this.route.data.subscribe((d: { user: User }) => {
      this.user = d.user;
      this.userPoints = d.user.points;
    });
    this.wheelService.getStatus().subscribe(status => {
      this.updateStatus(status)
      this.route.params.pipe(
        map(p => p["type"] == "b" ? "b" : "a"),
      ).subscribe((wheelType) => {
        this.wheelType = wheelType
        if (wheelType == 'a') {
          this.newWheel(this.status.wheelAPrizes)
        } else if (wheelType == 'b' && this.status.wheelBCost > 0) {
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
    this.spinLimit = status.wheelADailyLimit;
    let luckyVouchers = status.vouchers.filter(v => v instanceof LuckyPointVoucher);
    this.luckyPoints = luckyVouchers.reduce((total, l: LuckyPointVoucher) => total + l.currentValue, 0);
    this.totalPoints = luckyVouchers.reduce((total, l: LuckyPointVoucher) => total + l.totalValue, 0);
    this.ceilingCount = status.vouchers.filter(v => v.kind == VoucherKind.CeilingPrize).length;
    this.ceilingProgress = this.totalPoints - this.ceilingCount * this.status.ceilingCost;
    if (status.displayPrizes) {
      this.display1 = status.displayPrizes.slice(0, status.displayPrizes.length / 2);
      this.display2 = status.displayPrizes.slice(status.displayPrizes.length / 2);
    }
  }

  private newWheel(prizes: PrizeInfo[]) {
    let getStyle = (p: PrizeInfo, idx: number) => {
      if (p.isRealItem) {
        return this.wheelType == 'b' && (idx % 2 == 0) ? '#ff0000' : '#fe7676';
      } else if (p.isVoucher) {
        return '#f69562';
      } else if (p.isCoupon) {
        return (idx % 2 == 0) ? '#fe7676' : '#f69562';
      }
      return '#fff7e2';
    }
    let config = {
      'canvasId': 'spinwheel',
      'outerRadius': 150,
      'textFontSize': 18,
      'innerRadius': 28,
      'textOrientation': 'vertical',
      'textAlignment': 'outer',
      'numSegments': 12,
      'segments': prizes.map((p, idx) => {
        return {
          'fillStyle': getStyle(p, idx),
          'text': p.prizeName,
          'size': (1 / prizes.length)*360
        }
      }),
      'rotationAngle': 0,
    };
    this.wheel = new Winwheel(config);
    this.wheel.draw(true);
  }

  get hideWheel(): boolean {
    return this.wheelType == 'b' && this.status.wheelBCost <= 0;
  }

  confirmRedeemPoints() {
    this.dialog.open<RedeemPointsComponent, void, boolean>(RedeemPointsComponent)
      .afterClosed().subscribe(refresh => {
      if (!refresh) {
        return
      }
      this.wheelService.getStatus().subscribe(s => this.updateStatus(s));
    })
  }

  get canRedeemCeiling(): boolean {
    return this.status.ceilingCost <= this.luckyPoints;
  }

  confirmRedeemCeiling() {
    this.loading = true
    this.wheelService.redeemCeiling().subscribe(voucher => {
      this.wheelService.getStatus().subscribe(status => {
        this.loading = false
        this.dialog.open<RedeemCeilingComponent, IVoucher>(RedeemCeilingComponent, { data: voucher });
        this.updateStatus(status);
      })
    }, err => {
      this.loading = false;
      this.snackBar.open("兑换失败，请刷新重试。", null, { duration: 3000 });
    })
  }

  confirmRedeemCoupon() {
    let subject = new Subject<SpinWheelStatus>();
    subject.subscribe(r => this.updateStatus(r));
    this.dialog.open<RedeemCouponComponent, Subject<SpinWheelStatus>>(RedeemCouponComponent, { data: subject }).afterClosed().subscribe(_ => {
      subject.unsubscribe();
    })
  }

  get bjTimeNow(): moment.Moment {
    return moment().utcOffset(480)
  }

  get canSpin(): boolean {
    if (!this.isActive || this.loading || this.spinning || this.hideWheel) {
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
        remainSpin: this.user.isAdmanager() ? null : (this.spinLimit - todaySpin.length),
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
        } else if (prizeName.endsWith("（已折换）")) {
          prizeName = prizeName.substring(0, prizeName.length - 5)
        }
        for (let i = 0; i < prizes.length; i++) {
          if (prizes[i].prizeName === prizeName) {
            indexes.push(i)
          }
        }
        this.spin(indexes[Math.floor(Math.random() * indexes.length)] + 1, r);
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
      this.wheelService.getStatus().subscribe(s => this.updateStatus(s));
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
