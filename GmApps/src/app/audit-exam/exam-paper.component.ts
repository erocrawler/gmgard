import { Subject, of, zip } from "rxjs";

import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute, Params } from "@angular/router";

import { ExamService } from "./exam.service";
import { Exam, QuestionType } from "./exam";
import { ExamResult, ExamAnswer } from "../models/ExamResult";
import { ExamSubmission, QuestionSubmission } from "../models/ExamSubmission";
import { tap, switchMap, filter, debounceTime } from "rxjs/operators";

@Component({
  selector: "app-exam-paper",
  templateUrl: "./exam-paper.component.html",
  styleUrls: ["./exam-paper.component.css"]
})
export class ExamPaperComponent implements OnInit {

  constructor(protected service: ExamService, protected route: ActivatedRoute, protected router: Router) {
  }

  exam: Exam;
  loading = true;
  currentResult?: ExamResult;
  totalPoints: number;
  examActive: boolean;
  currentSubmission: ExamSubmission;
  answerMap = new Map<number, QuestionSubmission>();
  resultMap = new Map<number, ExamAnswer>();
  lastSave?: Date;
  private questionType = QuestionType;
  private saveDraftSubject = new Subject();

  ngOnInit() {
    this.route.params.pipe(
      switchMap((r: Params) => {
        const version = r["version"] || "";
        if (this.service.currentExamVersions().indexOf(version) < 0) {
          this.router.navigate(["/not-found"]);
          return of([null, null]);
        }
        return this.service.getExam(version).pipe(
          tap((exam: Exam) => this.exam = exam),
          switchMap(_ => zip(this.service.getExamDraft(version), this.service.getExamResult(version)))
        )
      })
    ).subscribe((result: [ExamSubmission, ExamResult]) => {
      if (result[1]) {
        this.processResult(result[1]);
        this.loading = false;
        return;
      }
      this.examActive = true;
      const submission: ExamSubmission = result[0] || { examAnswers: [], examVersion: this.exam.version };
      this.exam.questions.forEach(q => {
        if (QuestionType[q.type] === QuestionType.Information) {
          return;
        }
        let examAnswer = submission.examAnswers.find(m => m.questionId === q.id);
        if (!examAnswer) {
          examAnswer = { questionId: q.id, answer: "" };
          submission.examAnswers.push(examAnswer)
        }
        this.answerMap[examAnswer.questionId] = examAnswer;
      });
      this.currentSubmission = submission;
      this.saveDraftSubject.pipe(debounceTime(6000)).subscribe(_ => {
        this.service.saveExamDraft(this.currentSubmission)
          .pipe(filter(ok => ok)).subscribe(_ => this.lastSave = new Date());
      });
      this.loading = false;
    });
  }

  protected processResult(result: ExamResult) {
    this.currentResult = result;
    this.examActive = false;
    this.totalPoints = 0;
    result.examAnswers.forEach(a => {
      this.resultMap[a.questionId] = a;
      this.answerMap[a.questionId] = { answer: a.answer };
      this.totalPoints += a.point;
    });
  }

  setDirty() {
    this.saveDraftSubject.next();
  }

  confirmSubmit() {
    if (confirm("确认提交测试？提交后无法修改。")) {
      this.service.submitAnswer(this.currentSubmission).subscribe(r => {
        this.processResult(r);
        window.scrollTo(0, 0);
      });
    }
  }

}
