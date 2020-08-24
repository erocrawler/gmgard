import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute, NavigationEnd } from "@angular/router";
import { GachaService } from "./gacha.service";
import { GachaPool } from "app/gacha/gacha-pools";
import { first, filter } from "rxjs/operators"

@Component({
  templateUrl: "./gacha.component.html",
  styleUrls: ["./gacha.component.css"],
})
export class GachaComponent implements OnInit {

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private gachaService: GachaService,
    ) { }

    url: string;
    pools: GachaPool[] = [];
    loading = true;

    updateUrl() {
        this.route.firstChild.url.pipe(first()).subscribe(d => {
            if (d.length) {
                this.url = d[0].path;
            } else {
                this.route.params.pipe(first()).subscribe(p => this.url = p['showType'] || (this.pools.length ? this.pools[0].poolName : ""));
            }
        });
    }

    ngOnInit() {
        this.updateUrl();
        this.router.events.pipe(filter(f => f instanceof NavigationEnd))
          .subscribe((s: NavigationEnd) => this.updateUrl())
        this.gachaService.getPools().subscribe(pools => {
            this.loading = false;
            let p: GachaPool[] = [];
            pools.forEach(v => {
                if (!this.url) {
                    this.url = v.poolName;
                }
                if (v.poolName !== 'common') {
                    p.push(v);
                }
            });
            this.pools = p;
        })
  }
}
