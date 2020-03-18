
export interface EternalCircleStatus {
  progress: EternalCircleProgress
  retryCount: number;
}
export enum EternalCircleProgress {
  Prologue = 0,
  Act1Start,
  Act1After,
  ACT1End,
  Act2Start,
  Act2After,
  Act3,
  Act3_5,
  Act4,
  Act5,
  ActEX,
  GE,
  BE1,
  BE2,
  BE3,
  BE4,
  BE5,
  BE6,
  BE7,
  RE,
}
