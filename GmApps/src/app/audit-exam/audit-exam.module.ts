import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";

import { AppMaterialModule } from "../app-material.module";
import { AuditExamComponent } from "./audit-exam.component";
import { ExamIndexComponent } from "./exam-index.component";
import { ExamPaperComponent } from "./exam-paper.component";
import { ExamService } from "./exam.service";
import { BlankQuestionComponent } from "./question/blank-question.component";
import { ChoiceQuestionComponent } from "./question/choice-question.component";
import { TitleQuestionComponent } from "./question/title-question.component";
import { ExamRoutingModule } from "./exam-routing.module";
import { AdminExamPaperComponent } from "./admin/admin-exam-paper.component";

@NgModule({
    imports: [
        CommonModule,
        AppMaterialModule,
        FlexLayoutModule,
        FormsModule,
        HttpModule,
        ExamRoutingModule,
    ],
    declarations: [
        ExamIndexComponent,
        AuditExamComponent,
        ExamPaperComponent,
        BlankQuestionComponent,
        ChoiceQuestionComponent,
        TitleQuestionComponent,
        AdminExamPaperComponent,
    ],
    providers: [
        ExamService,
    ],
    bootstrap: [AuditExamComponent],
})
export class AuditExamModule { }
