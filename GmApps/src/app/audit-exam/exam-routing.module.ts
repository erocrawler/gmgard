import { NgModule } from "@angular/core";
import { RouterModule, Router, Routes, PreloadAllModules } from "@angular/router";

import { AuditExamComponent } from "./audit-exam.component";
import { ExamIndexComponent } from "./exam-index.component";
import { ExamPaperComponent } from "./exam-paper.component";
import { AdminExamPaperComponent } from "./admin/admin-exam-paper.component";
import { AdminGuard } from "../auth/admin-guard.service";

const routes: Routes = [
    {
        path: "",
        component: AuditExamComponent,
        children: [
            { path: "", component: ExamIndexComponent, pathMatch: "full" },
            { path: "do", component: ExamPaperComponent },
            { path: "admin", component: AdminExamPaperComponent, canActivate: [AdminGuard] },
        ]
    },

];
@NgModule({
    imports: [
        RouterModule.forChild(routes)
    ],
    exports: [RouterModule],
})
export class ExamRoutingModule { }
