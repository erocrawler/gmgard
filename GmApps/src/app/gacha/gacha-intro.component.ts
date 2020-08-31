import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { GameScenarios } from '../shared/adv-game/scenario';
import { August2020 } from './intro-scenarios';
import { AdvGameComponent } from '../shared/adv-game/adv-game.component';

@Component({
  selector: 'app-gacha-intro',
  templateUrl: './gacha-intro.component.html',
  styleUrls: ['./gacha-intro.component.css']
})
export class GachaIntroComponent implements OnInit, AfterViewInit {

  constructor(private activatedRoute: ActivatedRoute, private router: Router, private cd: ChangeDetectorRef) { }

  adv: GameScenarios<void>;
  @ViewChild(AdvGameComponent) game: AdvGameComponent<void>;
  showType: string

  ngOnInit(): void {
    this.showType = this.activatedRoute.snapshot.paramMap.get("showType")
    if (this.showType == "august2020") {
      this.adv = new GameScenarios<void>([August2020]);
    } else {
      this.exit();
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.game.playChapter(null), 4)
  }

  exit() {
    this.router.navigate(["/gacha", {showType: this.showType}]);
  }
}
