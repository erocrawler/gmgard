import { Component, OnInit } from "@angular/core";

import { ExamService } from "./exam.service";
import { ExamResult, ExamAnswer } from "../models/ExamResult";


@Component({
    selector: "exam-index",
    templateUrl: "./exam-index.component.html",
})
export class ExamIndexComponent implements OnInit {

    constructor(public service: ExamService) {
    }

    allExams: ExamResult[];

    ngOnInit() {
        this.service.getAllExamResult().subscribe(r => {
            const versions = this.service.currentExamVersions();
            this.allExams = r;
            if (!r) {
                this.allExams = [];
            }
            versions.forEach(v => {
                if (!this.allExams.find(er => er.examVersion === v)) {
                    const nr = new ExamResult();
                    nr.examVersion = v;
                    this.allExams.push(nr);
                }
            });
        })
    }
}
