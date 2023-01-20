import { Component, OnInit, AfterViewInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { GameScenarios } from '../shared/adv-game/scenario';

@Component({
  selector: 'app-gacha-intro',
  templateUrl: './gacha-intro.component.html',
  styleUrls: ['./gacha-intro.component.css']
})
export class GachaIntroComponent implements OnInit, AfterViewInit {

  constructor(private activatedRoute: ActivatedRoute, private router: Router) { }

  // TODO: Finish this next time.
  adv: GameScenarios;
  showType: string

  ngOnInit(): void {
    this.showType = this.activatedRoute.snapshot.paramMap.get("showType")
    this.exit();
  }

  ngAfterViewInit(): void {}

  exit() {
    this.router.navigate(["/gacha", {showType: this.showType}]);
  }
}
