
export interface GameStatus {
  progress: number;
  retryCount: number;
  newGameScenarioId: number;
  currentScenario: GameScenario;
}
export interface GameScenario {
  id: number;
  narrators: Narrator[];
  dialogs: Dialog[];
  next: Choice[];
}
export interface Choice {
  text: string;
  result: number;
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
