import { Observable } from "rxjs";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { BlogPreview } from "../models/Blog";
import { Paged } from "app/models/Paged";
import { map } from "rxjs/operators";

@Injectable()
export class BlogSearchService {

    constructor(private http: HttpClient) {
    }

    search(title: string): Observable<BlogPreview[]> {
        return this.http.get<Paged<BlogPreview>>("/api/Search/Blog", { params: { title: title, Limit: "5" }, withCredentials: true })
            .pipe(map(p => p ? p.items : []));
    }
}
