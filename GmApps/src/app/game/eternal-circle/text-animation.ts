import { Subscription, Subject, interval } from "rxjs";
import { takeWhile, map } from "rxjs/operators";

export class TextAnimation {
  private currentAnimation: Subscription
  private currentText = ""
  private currentDisplay = ""
  private playing = false;

  constructor(private textSubject: Subject<string>) { }

  get isPlaying() { return this.playing }

  play(text: string): Promise<boolean> {
    if (this.currentAnimation) {
      this.currentAnimation.unsubscribe();
    }
    this.playing = true;
    this.currentText = text;
    this.currentDisplay = "";
    let idx = 0;
    let obs = interval(1000 / text.length).pipe(
      map(() => {
        let char = text[idx];
        if (char == "<") {
          while (text[idx] != ">") {
            char += text[++idx];
          }
        }
        idx++;
        return char;
      }),
      takeWhile(() => idx <= text.length)
    )
    let p = new Subject<boolean>();
    this.currentAnimation = obs.subscribe((c) => {
      this.currentDisplay += c
      this.textSubject.next(this.currentDisplay)
    }, null, () => {
      this.playing = false;
      p.next(true);
      p.complete();
      });
    this.currentAnimation.add(() => { p.next(true); p.complete() });
    return p.toPromise();
  }

  skip() {
    if (this.currentAnimation) {
      this.currentAnimation.unsubscribe();
    }
    this.textSubject.next(this.currentText);
    this.playing = false;
  }
}
