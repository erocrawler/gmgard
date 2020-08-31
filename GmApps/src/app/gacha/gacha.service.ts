import { Observable, of, merge } from "rxjs";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { DomSanitizer, SafeStyle } from "@angular/platform-browser";
import { AngularIndexedDB } from "angular2-indexeddb";

import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { GachaRequest } from "../models/GachaRequest";
import { GachaResult, GachaStats, GachaItemDetails } from "../models/GachaResult";
import { GachaImageAnimation } from "./gacha-image-animation";
import { GachaVideoAnimation } from "./gacha-video-animation";
import { IGachaAnimation } from "./gacha-animation";
import { tap, map, scan } from "rxjs/operators";
import { GachaSetting } from "app/models/GachaSetting";
import { GachaPool, GACHA_POOLS } from "app/gacha/gacha-pools";

class Accumulator {
  total = 0;
  current = new Map<number, number>();
  accumulate(idx: number, progress: number) {
    let newTotal = 0;
    this.current.set(idx, progress).forEach(v => newTotal += v);
    this.total = newTotal / this.current.size;
  }
}

@Injectable()
export class GachaService {

  constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment, private sanitizer: DomSanitizer) {
    this.host = env.apiHost;
    this.db = new AngularIndexedDB("gmDB", 2);
    this.db.openDatabase(2, (evt: Event) => {
      const db: IDBDatabase = (evt.currentTarget as IDBRequest).result;
      if (db.objectStoreNames.contains("img")) {
        db.deleteObjectStore("img");
      }
      db.createObjectStore("img", { autoIncrement: false });
    })
  }

  private db: AngularIndexedDB;
  private host: string;
  private animationImages: HTMLImageElement[];
  private animationVideo: HTMLVideoElement;
  private lastResult: GachaResult = null;
  private stats: GachaStats;
  private pools: Map<string, GachaPool>;
  private playType: "none" | "low" | "high" = "none";

  supportAnimation(): boolean {
    return window.URL.createObjectURL && !!window.CanvasRenderingContext2D;
  }

  storeImageData(key: string, data: Blob) {
    this.db.add("img", data, key);
  }

  loadAnimation(type: "none" | "low" | "high"): Observable<number> {
    this.playType = type;
    if (type == "none") {
      return of(100);
    }
    if (type == "low") {
      return this.loadImage();
    }
    return this.loadVideo();
  }

  getAnimation(canvas: HTMLCanvasElement): IGachaAnimation {
    if (this.playType === "low" && this.animationImages && this.animationImages.length > 1) {
      return new GachaImageAnimation(canvas, this.animationImages);
    } else if (this.playType === "high" && this.animationVideo) {
      return new GachaVideoAnimation(canvas, this.animationVideo);
    }
    return null;
  }

  private loadImage(): Observable<number> {
    this.animationImages = GachaImageAnimation.getImages();
    const srcs = GachaImageAnimation.getSrcs();
    const animationImages = this.animationImages;
    const handleData = function (index: number, data: Blob) {
      //animationImages[index].onload = function () {
      //    window.URL.revokeObjectURL(animationImages[index].src);
      //}
      animationImages[index].src = window.URL.createObjectURL(data);
    }
    return this.makeRequest(srcs, handleData);
  }

  private loadVideo(): Observable<number> {
    this.animationVideo = document.createElement("video");
    const animationVideo = this.animationVideo;
    const handleData = function (index: number, data: Blob) {
      //animationVideo.onloadeddata = function () {
      //    window.URL.revokeObjectURL(animationVideo.src);
      //}
      animationVideo.src = window.URL.createObjectURL(data);
    }
    return this.makeRequest(["/assets/cards/gacha.mp4"], handleData);
  }

  gacha(pool: string, count: 1 | 10): Observable<GachaResult> {
    this.stats = null;
    const request: GachaRequest = { count, pool }
    return this.http.post<GachaResult>(this.host + "/api/Gacha/GetResult", request, {
      withCredentials: true
    }).pipe(tap(r => this.lastResult = r));
  }

  getStats(): Observable<GachaStats> {
    if (this.stats) {
      return of(this.stats);
    }
    return this.http.get<GachaStats>(this.host + "/api/Gacha/GetStats", { withCredentials: true })
      .pipe(tap(result => this.stats = result));
  }

  getPools(): Observable<Map<string, GachaPool>> {
    if (this.pools) {
      return of(this.pools);
    }
    return this.http.get<GachaSetting[]>(this.host + "/api/Gacha/GetCurrentPools", { withCredentials: true })
      .pipe(map(settings => {
        this.pools = new Map<string, GachaPool>();
        for (const s of settings) {
          const p = GACHA_POOLS[s.poolName];
          if (p) {
            this.pools.set(s.poolName, p);
          }
        }
        return this.pools;
      }));
  }

  getDetails(name: string): Observable<GachaItemDetails> {
    return this.getStats()
      .pipe(map((stats: GachaStats) => stats.userCards.find(i => i.name === name)));
  }

  getResult(): GachaResult {
    return this.lastResult;
  }

  getImages(): HTMLImageElement[] {
    return this.animationImages;
  }

  cardUrl(name: string): string {
    return `/assets/cards/${name}.png`;
  }

  cardCssUrl(name: string): SafeStyle {
    return this.sanitizer.bypassSecurityTrustStyle(`url(${this.cardUrl(name)})`);
  }

  private makeRequest(imageURIs: string[], handleData: (index: number, data: Blob) => void): Observable<number> {
    const db = this.db;

    const progress: Observable<[number, number]>[] = [];
    for (let i = 0; i < imageURIs.length; ++i) {
      progress.push(new Observable<[number, number]>(observer => {
        db.getByKey("img", imageURIs[i]).then((data: Blob) => {
          if (data) {
            handleData(i, data);
            observer.complete();
            return;
          }

          const request = new XMLHttpRequest();
          request.responseType = "blob";
          request.onload = function () {
            db.add("img", request.response, imageURIs[i]);
            handleData(i, request.response);
            observer.complete();
          };
          request.onerror = function (ev) { observer.error(ev) };
          request.onprogress = function (e: ProgressEvent) {
            if (e.lengthComputable) {
              observer.next([i, e.loaded / e.total * 100]);
            } else {
              observer.next([i, -1]);
            }
          }
          request.open("GET", imageURIs[i], true);
          request.send(null);
        });
      }));
    }
    return merge(...progress).pipe(
      scan((acc: Accumulator, val: [number, number]) => {
        acc.accumulate(val[0], val[1]);
        return acc;
      }, new Accumulator()),
      map(v => v.total));
  }
}
