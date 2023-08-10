import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, Subject } from "rxjs";
import { GameScenario, GameStatus, ExtraData, LocalChoiceData, ExtraDataKind, Choice, QuestionareData } from "../../models/GameScenario";
import { IGameScenarioProvider } from "../../shared/adv-game/scenario";
import { GameService, GameID } from "../game.service";

@Injectable({
  providedIn: 'root'
})
export class TarnishedWorldGameScenarioService implements IGameScenarioProvider {

  constructor(private gameService: GameService) {
  }

  status(): Observable<GameStatus> {
    return this.gameService.gameStatus(GameID.TarnishedWorld)
  }

  start(scene: GameScenario) {
    this.processData(scene);
    this.currentScene.next(scene);
  }

  restart(): Observable<GameStatus> {
    return this.gameService.gameRestart(GameID.TarnishedWorld);
  }

  jump(id: number): Observable<GameStatus> {
    return this.gameService.gameChapter(GameID.TarnishedWorld, id);
  }

  prev() {
    this.gameService.prevScene(GameID.EternalCircle).subscribe(r => this.currentScene.next(r));
  }

  questionare(answers: number[]) {
    this.gameService.questionare(GameID.TarnishedWorld, answers).subscribe(r => this.currentScene.next(r));
  }

  currentScene = new BehaviorSubject<GameScenario>(null);
  next(choiceId: number) {
    if (this.localChoiceData) {
      this.localChoiceData.vistedChoices.add(choiceId);
      const tempScene: GameScenario = {
        dialogs: [...this.localChoiceData.localChoice.resultMap[choiceId]],
        narrators: this.localChoiceData.scene.narrators,
        next: [],
        id: choiceId,
        data: null,
      }
      if (choiceId === this.localChoiceData.localChoice.exitChoice) {
        tempScene.next = this.localChoiceData.sceneChoices;
        this.localChoiceData = null;
      } else {
        tempScene.next = this.filterLocalChoices();
      }
      this.currentScene.next(tempScene);
      return;
    } else if (this.questionareData) {
      if (choiceId >= 0) {
        this.questionareData.selectedAnswers.push(choiceId);
      }
      if (this.questionareData.selectedAnswers.length >= this.questionareData.questionare.questions.length) {
        this.questionare(this.questionareData.selectedAnswers);
        this.questionareData = null;
        return;
      }
      const currentQuestion = this.questionareData.questionare.questions[this.questionareData.selectedAnswers.length];
      const tempScene: GameScenario = {
        dialogs: [{
          bgImg: currentQuestion.bgImg ?? this.questionareData.scene.dialogs[this.questionareData.scene.dialogs.length - 1].bgImg,
          texts: [["", currentQuestion.question]],
          effect: [],
        }],
        narrators: [],
        next: currentQuestion.options.map((o, i) => ({ text: o, result: i+1 })),
        id: choiceId,
        data: null,
      }
      this.currentScene.next(tempScene);
      return;
    }
    this.gameService.nextScene(GameID.TarnishedWorld, choiceId).subscribe(r => {
      this.processData(r);
      this.currentScene.next(r)
    });
  }

  private processData(scene: GameScenario) {
    if (!scene.data) {
      return;
    }
    if (scene.data.kind === ExtraDataKind.LocalChoice) {
      this.localChoiceData = {
        localChoice: scene.data as LocalChoiceData,
        scene: scene,
        sceneChoices: scene.next,
        vistedChoices: new Set<number>(),
      }
      scene.next = this.filterLocalChoices();
    } else if (scene.data.kind == ExtraDataKind.Qustionare) {
      this.questionareData = {
        questionare: scene.data as QuestionareData,
        scene: scene,
        selectedAnswers: [],
      }
      scene.next[0].result = -1;
    }
  }

  private filterLocalChoices(): Choice[] {
    return this.localChoiceData.localChoice.choices.filter(v => {
      const requirements = this.localChoiceData.localChoice.showAfterVisitChoices[v.result];
      if (!requirements) {
        return true;
      }
      for (const id of requirements) {
        if (!this.localChoiceData.vistedChoices.has(id)) {
          return false;
        }
      }
      return true;
    })
  }

  private localChoiceData: {
    localChoice: LocalChoiceData;
    scene: GameScenario;
    sceneChoices: Choice[];
    vistedChoices: Set<number>;
  };
  private questionareData: {
    questionare: QuestionareData;
    scene: GameScenario;
    selectedAnswers: number[];
  }
}
