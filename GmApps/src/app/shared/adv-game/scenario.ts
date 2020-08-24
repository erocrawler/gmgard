
export class GameScenarios<P> {
  constructor(s :Scenario<P>[]) {
    this.scenes = s.reduce((v, cur) => { v.set(cur.title, cur); return v; }, new Map<P, Scenario<P>>());
  }

  private scenes: Map<P, Scenario<P>>
  currentDialog = 0;
  currentText = 0;
  currentScene: Scenario<P>
  currentEffects: Map<number, Effects>;

  getScene(progress: P): Scenario<P> {
      return this.scenes.get(progress)
  }

  setScene(progress: P) {
    this.currentDialog = 0;
    this.currentScene = this.getScene(progress);
    this.initDialog();
  }

  getChoice(): Choice<P>[] {
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

export interface Narrator {
  avatar: string
  name: string
  display?: string
}

export interface Dialog {
  bgImg: string
  texts: [string, string][]
  effect?: { pos: number, kind: Effects }[]
}

export interface Scenario<P> {
  title: P
  dialogs: Dialog[]
  narrators: Narrator[]
  next?: Choice<P>[]
  prev?: P
}

export interface Choice<P> {
  text: string
  result: P
}

export enum Effects {
  NONE,
  SPARK,
  SHAKE,
  FLASH,
  TITLE,
  CENTER_BG,
}
