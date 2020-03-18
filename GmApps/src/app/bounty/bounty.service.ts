import { Observable } from "rxjs/Observable";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { Paged } from "../models/Paged";
import { BountyPreview, BountyShowType } from "../models/Bounty";

@Injectable()
export class BountyService {

    private host: string;

    constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
        this.host = env.apiHost;
    }

    list(showType: BountyShowType, page: number = 1): Observable<Paged<BountyPreview>> {
      return this.http.get<Paged<BountyPreview>>(this.host + "/api/Bounty/List", { params: { page: String(page), showType: showType }, withCredentials: true });
    }

    //get(id: number): Observable<BountyDetails> {
    //    return this.http.get(this.host + '/api/Bounty/Details', { params: { id: id }, withCredentials: true })
    //        .map(resp => resp.json() as BountyPreview[]);
    //}

}
