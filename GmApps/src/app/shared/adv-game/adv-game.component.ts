import { Component, OnInit, HostListener, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Effects, Narrator, Choice, GameScenarios } from './scenario';
import { TextAnimation } from './text-animation';
import { debounceTime, delay } from 'rxjs/operators';
import { fromEvent, Subject, Subscription, of } from 'rxjs';

@Component({
  selector: 'app-adv-game',
  templateUrl: './adv-game.component.html',
  styleUrls: ['./adv-game.component.css'],
  exportAs: 'appAdvGame'
})
export class AdvGameComponent<P> implements OnInit, OnDestroy {

  constructor(private sanitizer: DomSanitizer) { }

  @Input() scenarios: GameScenarios<P>
  @Output() choice = new EventEmitter<P>();
  @Output() ended = new EventEmitter<void>();
  @Output() exit = new EventEmitter<void>();
  bgimg1: string
  bgimg2: string
  show1 = true
  hasImg = false;
  narrator?: Narrator
  waitClick = false
  waitChoose = false
  choices: Choice<P>[]
  text: SafeHtml
  currentEffect: string
  textAnimation: TextAnimation;
  textSubject = new Subject<string>();
  isEnded = false;
  title: string = null;
  imgWidth = 800;
  imgHeight = 500;
  imgSub: Subscription;
  loading = true;
  loadingMode = "indeterminate";
  loadingProgress = 0;
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
    this.scenarios.currentScene.narrators.forEach(n => loadImg(n.avatar))
    this.scenarios.currentScene.dialogs.forEach(n => loadImg(n.bgImg));
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

  playChapter(progress: P) {
    this.scenarios.setScene(progress);
    this.choices = null;
    this.waitChoose = false;
    this.hasImg = false;
    this.preloadImg();
    this.play();
  }

  handleChoice(ev: MouseEvent, choice: P) {
    if (ev) {
      ev.stopPropagation();
    }
    this.choice.next(choice);
  }

  handlePrev(ev: MouseEvent) {
    this.isEnded = false;
    this.handleChoice(ev, this.scenarios.currentScene.prev);
  }

  play() {
    this.waitClick = false;
    let dialog = this.scenarios.getDialog();
    if (!dialog) {
      dialog = this.scenarios.nextDialog();
    }
    if (!dialog) {
      this.nextScene();
      return;
    }
    if (!this.hasImg) {
      this.showImg(dialog.bgImg);
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
    const [n, text, effect] = this.scenarios.nextText();
    this.narrator = n;
    this.textAnimation.play(text).then(_ => this.waitClick = true);
    if (effect != Effects.NONE) {
      let wait = false;
      switch (effect) {
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
  }

  nextScene() {
    let chocies = this.scenarios.getChoice();
    if (chocies.length == 0) {
      this.isEnded = true;
      this.ended.next();
      return;
    }
    if (chocies.length == 1) {
      this.handleChoice(null, chocies[0].result)
      return;
    }
    this.choices = chocies;
    this.waitChoose = true;
  }

  handleExit() {
    this.exit.next();
  }
}
