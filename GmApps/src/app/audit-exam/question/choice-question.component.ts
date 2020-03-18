import { Component, OnInit, Input, Output, EventEmitter, ViewChildren, QueryList } from "@angular/core";
import { MatCheckbox } from "@angular/material";

import { Question } from "../exam";
import { QuestionSubmission } from "../../models/ExamSubmission";
import { ExamAnswer } from "../../models/ExamResult";

@Component({
  selector: "choice-question",
  templateUrl: "./choice-question.component.html",
  styleUrls: ["./question.component.css"],
})
export class ChoiceQuestionComponent implements OnInit {

    @Input()
    question: Question;

    @Input()
    answer: string;

    @Output()
    answerChange = new EventEmitter<string>();

    @ViewChildren(MatCheckbox)
    checkboxes: QueryList<MatCheckbox>;

    @Input()
    active: boolean;

    @Input()
    result: ExamAnswer;

    selected: number[] = [];

    constructor() { }

    ngOnInit() {
        if (!this.active && this.result) {
            this.answer = this.result.answer;
        }
        if (this.answer) {
            this.selected = JSON.parse(this.answer) as number[];
        }
    }

    updateAnswer() {
        const values = this.checkboxes.filter(item => item.checked).map(item => item.value);
        this.answerChange.emit(JSON.stringify(values));
    }

}
