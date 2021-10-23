import { Observable } from "rxjs";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Paged } from "../models/Paged";
import { BountyPreview, BountyShowType } from "../models/Bounty";

@Injectable()
export class BountyService {

    constructor(private http: HttpClient) {
    }

    list(showType: BountyShowType, page: number = 1): Observable<Paged<BountyPreview>> {
      return this.http.get<Paged<BountyPreview>>("/api/Bounty/List", { params: { page: String(page), showType: showType }, withCredentials: true });
    }

    //get(id: number): Observable<BountyDetails> {
    //    return this.http.get(this.host + '/api/Bounty/Details', { params: { id: id }, withCredentials: true })
    //        .map(resp => resp.json() as BountyPreview[]);
    //}

}
