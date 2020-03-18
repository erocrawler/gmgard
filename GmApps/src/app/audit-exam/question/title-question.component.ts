import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";

import { Question } from "../exam";
import { QuestionSubmission } from "../../models/ExamSubmission";
import { ExamAnswer } from "../../models/ExamResult";

@Component({
  selector: "title-question",
  templateUrl: "./title-question.component.html",
  styleUrls: ["./question.component.css"],
})
export class TitleQuestionComponent implements OnInit {
    @Input()
    question: Question;

    @Input()
    answer: string;

    @Input()
    active: boolean;

    @Input()
    result: ExamAnswer;

    @Output()
    answerChange = new EventEmitter<string>();

    constructor() { }

    ngOnInit() {
        if (!this.active && this.result) {
            this.answer = this.result.answer;
        }
    }

    emit(val: string) {
        this.answerChange.emit(val);
    }
}

