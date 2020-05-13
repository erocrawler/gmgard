
export enum VoucherKind {
  Unknown = 0,
  WheelA,
  WheelB,
  LuckyPoint,
  Prize,
  CeilingPrize,
  Coupon,
}

export interface IVoucher {
	voucherID: string;
	issueTime: Date;
	useTime?: Date;
	redeemItem: string;
  kind: VoucherKind;
  userName?: string;
}

export interface PrizeInfo {
  prizeName: string;
  prizePic: string;
  isRealItem: boolean;
  isVoucher: boolean;
  prizeLPValue: number;
  itemLink?: string;
}

export interface SpinWheelStatus {
  title: string;
  isActive: boolean;
  userPoints: number;
  vouchers: IVoucher[];
  wheelAPrizes: PrizeInfo[];
  wheelBPrizes: PrizeInfo[];
  displayPrizes: PrizeInfo[];
  couponPrizes: PrizeInfo[];
  wheelACost: number;
  wheelBCost: number;
  ceilingCost: number;
  showRedeem: boolean;
  wheelADailyLimit: number;
}

export interface SpinWheelResult {
  prize: PrizeInfo;
  voucher: IVoucher;
}

export interface StockInfo {
  prizeName: string;
  stock?: IVoucher[];
}

export function newVoucher(v: IVoucher): Voucher {
  switch (v.kind) {
    case VoucherKind.WheelA:
    case VoucherKind.WheelB:
      return new SpinWheelVoucher(v);
    case VoucherKind.CeilingPrize:
    case VoucherKind.Prize:
    case VoucherKind.Coupon:
      return new PrizeVoucher(v);
    case VoucherKind.LuckyPoint:
      return new LuckyPointVoucher(v);
  }
  throw new Error("Unknown voucher kind: " + v.kind);
}

export abstract class Voucher implements IVoucher {
  voucherID: string;
  issueTime: Date;
  useTime?: Date;
  redeemItem: string;
  kind: VoucherKind;
  userName?: string
  constructor(v: IVoucher) {
    this.voucherID = v.voucherID;
    this.issueTime = v.issueTime;
    this.useTime = v.useTime;
    this.redeemItem = v.redeemItem;
    this.kind = v.kind;
    this.userName = v.userName;
  }
}

export class SpinWheelVoucher extends Voucher {
  kind: VoucherKind.WheelA | VoucherKind.WheelB;
}

export class PrizeVoucher extends Voucher {
  kind: VoucherKind.Prize | VoucherKind.CeilingPrize | VoucherKind.Coupon;
  get isUsed(): boolean {
    return !!this.useTime;
  }
}

export class LuckyPointVoucher extends Voucher {
  kind: VoucherKind.LuckyPoint;
  currentValue: number
  totalValue: number
  constructor(v: IVoucher) {
    super(v);
    this.currentValue = Number(v.redeemItem.split('/')[0]);
    this.totalValue = Number(v.redeemItem.split('/')[1]);
    this.redeemItem = "幸运积分" + v.redeemItem;
  }
  get usedValue() {
    return this.totalValue - this.currentValue;
  }
}
