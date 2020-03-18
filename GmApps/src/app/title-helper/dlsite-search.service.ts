import { Observable } from "rxjs/Observable";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { DLsite } from "../models/DLsite";

@Injectable()
export class DlsiteSearchService {

    private host: string;

    constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
        this.host = env.apiHost;
    }

    search(title: string): Observable<DLsite[]> {
        return this.http.post<{ success: boolean, entries: DLsite[] }>(
            this.host + "/api/Search/Dlsite",
            JSON.stringify(title),
            { withCredentials: true, headers: { "Content-Type": "application/json" } })
            .map(resp => {
                if (resp.success) {
                    return (resp.entries || []) as DLsite[];
                }
                throw new Error();
            });
    }
}
