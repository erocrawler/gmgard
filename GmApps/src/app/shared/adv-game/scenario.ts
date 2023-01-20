import { BehaviorSubject, Observable, Subject } from "rxjs";
import { GameScenario, Choice, Dialog, Narrator, Effects } from "../../models/GameScenario";

export interface IGameScenarioProvider {
  currentScene: Observable<GameScenario>
  next(choiceId: number)
  prev()
}

export class GameScenarios {
  constructor(private sceneProvider: IGameScenarioProvider) {
    sceneProvider.currentScene.subscribe(c => {
      if (!c) {
        return;
      }
      this.currentScene = c;
      this.currentDialog = 0;
      this.initDialog();
      this.sceneReadySbj.next(true);
    });
  }

  private sceneReadySbj = new BehaviorSubject<boolean>(false);
  private currentDialog = 0;
  private currentText = 0;
  private currentScene: GameScenario
  private currentEffects: Map<number, Effects>;

  get sceneReady(): Observable<boolean> {
    return this.sceneReadySbj;
  }

  nextScene(progress: number) {
    this.sceneReadySbj.next(false);
    this.sceneProvider.next(progress);
  }

  prevScene() {
    this.sceneReadySbj.next(false);
    this.sceneProvider.prev();
  }

  getAssets(): string[] {
    return this.currentScene.dialogs.map(d => d.bgImg).concat(this.currentScene.narrators.map(d => d.avatar));
  }

  getChoice(): Choice[] {
    if (!this.currentScene.next) {
      return [];
    }
    return this.currentScene.next;
  }

  private initDialog() {
    this.currentText = 0;
    let dialog = this.currentScene.dialogs[this.currentDialog];
    if (dialog && dialog.effect) {
      this.currentEffects = dialog.effect.reduce((v, cur) => { v.set(cur.pos, cur.kind); return v; }, new Map<number, Effects>())
    } else {
      this.currentEffects = null;
    }
  }

  nextDialog(): Dialog {
    if (this.currentScene.dialogs.length <= this.currentDialog) {
      return;
    }
    this.currentDialog++;
    this.initDialog();
    return this.getDialog();
  }

  getDialog(): Dialog {
    if (this.currentScene.dialogs.length <= this.currentDialog
      || this.currentText >= this.currentScene.dialogs[this.currentDialog].texts.length) {
      return null;
    }
    return this.currentScene.dialogs[this.currentDialog];
  }

  nextText(): [Narrator, string, Effects] {
    let d = this.getDialog();
    if (!d) {
      return null;
    }
    const [name, text] = d.texts[this.currentText]
    let e = Effects.NONE;
    if (this.currentEffects && this.currentEffects.has(this.currentText)) {
      e = this.currentEffects.get(this.currentText);
    }
    this.currentText++;
    return [this.currentScene.narrators.find(v => v.name == name), text, e];
  }
}
