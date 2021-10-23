import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpResponse } from "@angular/common/http";
import { IVoucher, Voucher, newVoucher, SpinWheelStatus, SpinWheelResult, StockInfo, PrizeInfo } from "../models/Vouchers";
import { Observable, forkJoin } from "rxjs";
import { map } from "rxjs/operators";

export declare type StatusAndVouchers = { status: SpinWheelStatus, vouchers: IVoucher[]};

@Injectable()
export class WheelService {
  constructor(private http: HttpClient) {
  }

  getVouchers(): Observable<IVoucher[]> {
    return this.http.get<IVoucher[]>("/api/Wheel/Get", { withCredentials: true })
      .pipe(map(s => s.map(newVoucher)));
  }

  getStatus(): Observable<SpinWheelStatus> {
    return this.http.get<SpinWheelStatus>("/api/Wheel/Status", { withCredentials: true });
  }

  getStatusAndVoucher(): Observable<StatusAndVouchers> {
    return forkJoin({
      status: this.getStatus(),
      vouchers: this.getVouchers()
    });
  }

  spin(type: string): Observable<SpinWheelResult> {
    return this.http.post<SpinWheelResult>("/api/Wheel/Spin", null, { params: { "wheelType": type }, withCredentials: true });
  }

  getStock(): Observable<StockInfo[]> {
    return this.http.get<StockInfo[]>("/api/Wheel/Stock", { withCredentials: true });
  }

  getVoucher(id: string): Observable<IVoucher[]> {
    return this.http.get<IVoucher[]>("/api/Wheel/Voucher", { params: { id }, withCredentials: true });
  }

  getForUser(name: string): Observable<IVoucher[]> {
    return this.http.get<IVoucher[]>("/api/Wheel/GetForUser", { params: { name }, withCredentials: true });
  }

  addStock(name: string, count: number): Observable<HttpResponse<{}>> {
    return this.http.post("/api/Wheel/Stock", null, { params: { prizeName: name, count: count.toString() }, withCredentials: true, observe: "response" });
  }

  redeemCeiling(): Observable<IVoucher> {
    return this.http.post<IVoucher>("/api/Wheel/RedeemCeiling", null, { withCredentials: true });
  }

  redeemPoints(id: string): Observable<PrizeInfo> {
    return this.http.post<PrizeInfo>("/api/Wheel/RedeemPoints", null, { params: { voucherId: id }, withCredentials: true });
  }

  redeemCoupon(val: number): Observable<SpinWheelResult> {
    return this.http.post<SpinWheelResult>("/api/Wheel/RedeemCoupon", null, { params: { "spentPoints": val.toString() }, withCredentials: true });
  }

  exchange(id: string): Observable<IVoucher> {
    return this.http.post<IVoucher>("/api/Wheel/Exchange", null, { params: { voucherId: id }, withCredentials: true });
  }

  markRedeemed(id: string): Observable<{}> {
    return this.http.post<{}>("/api/Wheel/MarkRedeemed", null, { params: { voucherId: id }, withCredentials: true });
  }
}
