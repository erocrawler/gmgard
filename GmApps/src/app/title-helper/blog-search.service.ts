import { Observable } from "rxjs/Observable";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { BlogPreview } from "../models/Blog";
import { Paged } from "app/models/Paged";
import { map } from "rxjs/operators";

@Injectable()
export class BlogSearchService {

    private host: string;

  constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
        this.host = env.apiHost;
    }

    search(title: string): Observable<BlogPreview[]> {
        return this.http.get<Paged<BlogPreview>>(this.host + "/api/Search/Blog", { params: { title: title, Limit: "5" }, withCredentials: true })
            .pipe(map(p => p ? p.items : []));
    }
}
