import { of, zip } from "rxjs";
import { catchError, switchMap } from "rxjs/operators";
import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute, Params } from "@angular/router";

import { ExamService } from "../exam.service";
import { ExamPaperComponent } from "../exam-paper.component";

@Component({
  selector: "admin-exam-paper",
  templateUrl: "./admin-exam-paper.component.html",
  styleUrls: ["./admin-exam-paper.component.css"]
})
export class AdminExamPaperComponent extends ExamPaperComponent implements OnInit {

    constructor(protected service: ExamService, protected route: ActivatedRoute, protected router: Router) {
        super(service, route, router);
    }

    username = "";
    requestActive: boolean;
    version = "";
    allVersions: string[];

    ngOnInit() {
        this.allVersions = this.service.currentExamVersions();
        this.examActive = false;
        this.route.params.pipe(switchMap((r: Params) => {
            this.username = r["user"] || "";
            this.version = r["version"] || "";
            if (this.username && this.version) {
                this.requestActive = true;
              return zip(this.service.getExam(this.version), this.service.getExamResultForUser(this.username, this.version))
                    .pipe(catchError(err => {
                        console.log(err);
                        return of(null);
                    }));
            }
            return of(null);
        })).subscribe(result => {
            this.requestActive = false;
            if (!result || !result[1]) {
                this.currentResult = null;
                return;
            }
            this.exam = result[0];
            this.processResult(result[1]);
        });
    }

    lookup(username: string, version: string) {
        if (!version) {
            return;
        }
        this.router.navigate(["audit-exam", "admin", { user: username, version: version }]);
    }

}
