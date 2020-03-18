import { Component, OnInit, HostListener, OnDestroy } from '@angular/core';
import { Scenarios, Dialog, Scenario, Choice, Narrator, Effects } from './scenario';
import { TextAnimation } from './text-animation';
import { Subject, of, fromEvent, Subscription } from 'rxjs';
import { delay, debounceTime } from 'rxjs/operators';
import { EternalCircleStatus, EternalCircleProgress } from 'app/models/EternalCircleStatus';
import { GameService } from '../game.service';
import { DomSanitizer, SafeStyle, SafeHtml } from '@angular/platform-browser';

@Component({
  templateUrl: './play.component.html',
  styleUrls: ['./play.component.css'],
})
export class PlayComponent implements OnInit, OnDestroy {

  constructor(private gameService: GameService, private sanitizer: DomSanitizer) { }

  scenarios: Map<EternalCircleProgress, Scenario>
  currentScene: Scenario
  dialog: Dialog
  bgimg1: string
  bgimg2: string
  show1 = true
  narrator?: Narrator
  waitClick = false
  waitChoose = false
  choices: Choice[]
  text: SafeHtml
  currentDialog = 0;
  currentText = 0;
  currentEffect: string
  effects: Map<number, Effects>
  textAnimation: TextAnimation;
  textSubject = new Subject<string>();
  ended = false;
  title: string = null;
  imgWidth = 800;
  imgHeight = 500;
  imgSub: Subscription;
  loading = true;
  loadingMode = "indeterminate";
  loadingProgress = 0;
  status: EternalCircleStatus;
  imgCache: HTMLImageElement[] = [];

  ngOnInit() {
    this.imgSub = fromEvent(window, "resize").pipe(
      debounceTime(500)
    ).subscribe(_ => {
      this.resize();
    });
    this.resize();
    this.textAnimation = new TextAnimation(this.textSubject);
    this.textSubject.subscribe(s => { this.text = this.sanitizer.bypassSecurityTrustHtml(s) });
    this.scenarios = Scenarios.reduce((v, cur) => { v.set(cur.title, cur); return v; }, new Map<EternalCircleProgress, Scenario>());
    this.gameService.eternalCircleStatus().subscribe(r => {
      this.status = r
      this.currentScene = this.scenarios.get(r.progress);
      this.preloadImg();
      this.play();
    })
  }

  ngOnDestroy() {
    this.imgSub.unsubscribe();
  }

  preloadImg() {
    this.loading = true;
    this.loadingMode = "determinate";
    this.loadingProgress = 0;
    const imgs = new Set<string>();
    let imgLoaded = 0;
    let loadImg = (i: string) => {
      if (imgs.has(i)) {
        return;
      }
      const img = new Image();
      img.src = i;
      img.onload = () => {
        imgLoaded++;
        this.loadingProgress = (imgLoaded / imgs.size) * 100;
        if (this.loadingProgress >= 100) {
          this.loading = false;
        }
      }
      imgs.add(i);
      this.imgCache.push(img);
    }
    this.currentScene.narrators.forEach(n => loadImg(n.avatar))
    this.currentScene.dialogs.forEach(n => loadImg(n.bgImg));
  }

  @HostListener('document:keypress', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.keyCode == 13 || event.keyCode == 32) {
      this.handleClick();
    }
  }

  resize() {
    if (window.innerWidth >= 800) {
      this.imgWidth = 800;
    } else {
      this.imgWidth = window.innerWidth;
    }
    this.imgHeight = this.imgWidth / 8 * 5
  }

  handleClick() {
    if (this.textAnimation.isPlaying) {
      this.textAnimation.skip();
      return;
    }
    if (!this.waitClick || this.waitChoose) {
      return
    }
    this.play();
  }

  handleChoice(ev: MouseEvent, choice: EternalCircleProgress) {
    if (ev) {
      ev.stopPropagation();
    }
    this.currentScene = this.scenarios.get(choice);
    if (this.status.retryCount > 0 && choice == EternalCircleProgress.ActEX) {
      this.currentScene = this.scenarios.get(EternalCircleProgress.Act5);
    }
    this.preloadImg();
    this.gameService.eternalCircleProgress(this.currentScene.title).subscribe(_ => {
      this.waitChoose = false;
      this.choices = null;
      this.currentDialog = 0;
      this.currentText = 0;
      this.play();
    })
  }

  handlePrev(ev: MouseEvent) {
    this.status.retryCount++;
    this.ended = false;
    this.handleChoice(ev, this.currentScene.prev);
  }

  play() {
    this.waitClick = false;
    if (!this.dialog || this.currentText >= this.dialog.texts.length) {
      this.nextDialog();
      if (!this.dialog) {
        this.nextScene();
        return;
      }
      if (this.dialog) {
        this.showImg(this.dialog.bgImg);
      }
      this.currentText = 0;
    }
    if (!this.dialog) {
      return
    }
    this.playText();
  }

  showImg(bgimg: string) {
    if (this.show1) {
      if (this.bgimg1 == bgimg) {
        return;
      }
      this.bgimg2 = bgimg;
    } else {
      if (this.bgimg2 == bgimg) {
        return;
      }
      this.bgimg1 = bgimg;
    }
    this.show1 = !this.show1;
  }

  playText() {
    this.text = this.sanitizer.bypassSecurityTrustHtml("");
    this.currentEffect = "";
    const [name, text] = this.dialog.texts[this.currentText];
    this.narrator = this.currentScene.narrators.find(v => v.name == name)
    this.textAnimation.play(text).then(_ => this.waitClick = true);
    if (this.effects && this.effects.has(this.currentText)) {
      let wait = false;
      switch (this.effects.get(this.currentText)) {
        case Effects.FLASH:
          this.currentEffect = "flash";
          break;
        case Effects.SHAKE:
          this.currentEffect = "shake";
          break;
        case Effects.SPARK:
          this.currentEffect = "spark";
          wait = true;
          break;
        case Effects.TITLE:
          this.currentEffect = "title";
          this.title = text;
          wait = true;
          break;
        case Effects.CENTER_BG:
          this.currentEffect = "center-bg";
          wait = true;
          break;
      }
      if (!wait) {
        of('').pipe(delay(1000)).subscribe(_ => this.currentEffect = '');
      }
    }
    this.currentText++;
  }

  nextDialog() {
    if (this.currentScene.dialogs.length <= this.currentDialog) {
      this.dialog = null;
      return;
    }
    this.currentText = 0;
    this.dialog = this.currentScene.dialogs[this.currentDialog++];
    if (this.dialog && this.dialog.effect) {
      this.effects = this.dialog.effect.reduce((v, cur) => { v.set(cur.pos, cur.kind); return v; }, new Map<number, Effects>())
    } else {
      this.effects = null;
    }
  }

  nextScene() {
    if (!this.currentScene.next) {
      this.ended = true;
      return;
    }
    if (this.currentScene.next.length == 1) {
      this.handleChoice(null, this.currentScene.next[0].result)
      return;
    }
    this.choices = this.currentScene.next;
    this.waitChoose = true;
  }

}
