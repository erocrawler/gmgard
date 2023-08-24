
export interface GameStatus {
  progress: number;
  retryCount: number;
  newGameScenarioId: number;
  currentScenario: GameScenario;
  inventory: Inventory[];
  chapters: Chapter[];
}
export interface Chapter {
  id: number;
  name: string;
}
export interface Inventory {
  name: string;
  description: string;
}
export interface GameScenario {
  id: number;
  narrators: Narrator[];
  dialogs: Dialog[];
  next: Choice[];
  data: ExtraData
}
export interface Choice {
  text: string;
  result: number;
  locked?: boolean;
}
export interface Narrator {
  avatar: string;
  name: string;
  display: string;
}
export interface Dialog {
  bgImg: string;
  texts: string[][];
  effect: DialogEffect[];
}
export interface DialogEffect {
  pos: number;
  kind: Effects;
}
export const enum Effects {
  NONE,
  SPARK,
  SHAKE,
  FLASH,
  TITLE,
  CENTER_BG,
}
export interface ExtraData {
  kind: string;
}
export namespace ExtraDataKind {
  export const LocalChoice = "local_choice";
  export const Qustionare = "questionare";
}
export interface LocalChoiceData {
  kind: "local_choice";
  choices: Choice[];
  resultMap: {
    [key: string]: Dialog[]
  };
  showAfterVisitChoices: {
    [key: string]: number[]
  };
  exitChoice: number;
}

export interface QuestionareData {
  kind: "questionare";
  questions: {
    question: string,
    options: string[],
    bgImg?: string,
  }[];
}
