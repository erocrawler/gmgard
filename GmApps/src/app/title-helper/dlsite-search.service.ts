import { Observable } from "rxjs";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { DLsite } from "../models/DLsite";
import { map } from "rxjs/operators";

@Injectable()
export class DlsiteSearchService {

    constructor(private http: HttpClient) {
    }

    search(title: string): Observable<DLsite[]> {
        return this.http.post<{ success: boolean, entries: DLsite[] }>(
            "/api/Search/Dlsite",
            JSON.stringify(title),
            { withCredentials: true, headers: { "Content-Type": "application/json" } })
            .pipe(map(resp => {
                if (resp.success) {
                    return (resp.entries || []) as DLsite[];
                }
                throw new Error();
            }));
    }
}
