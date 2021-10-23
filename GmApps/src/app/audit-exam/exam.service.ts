import { Observable } from "rxjs";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { Exam } from "./exam";
import { ExamSubmission } from "../models/ExamSubmission";
import { ExamResult } from "../models/ExamResult";
import { map } from "rxjs/operators";

@Injectable()
export class ExamService {

    constructor(private http: HttpClient) {
    }

    currentExamVersions(): string[] {
        return ["201705", "201707"];
    }

    getExam(version: string): Observable<Exam> {
        return this.http.get<Exam>(`/assets/audit-exam-${version}.json`);
    }

    saveExamDraft(draft: ExamSubmission): Observable<boolean> {
        return this.http.put("/api/AuditExam/Draft", draft, { withCredentials: true, observe: "response" })
            .pipe(map(resp => resp.ok));
    }

    getExamDraft(version: string): Observable<ExamSubmission> {
        return this.http.get<ExamSubmission>("/api/AuditExam/Draft", { params: { version: version }, withCredentials: true });
    }

    submitAnswer(submission: ExamSubmission): Observable<ExamResult> {
        return this.http.post<ExamResult>("/api/AuditExam/Submit", submission, { withCredentials: true });
    }

    getExamResult(version: string): Observable<ExamResult|null> {
        return this.http.get<ExamResult>("/api/AuditExam/Result", { params: { version: version }, withCredentials: true });
    }

    getAllExamResult(): Observable<ExamResult[] | null> {
        return this.http.get<ExamResult[]>("/api/AuditExam/Results", { withCredentials: true });
    }

    // Admin Only.
    getExamResultForUser(user: string, version: string): Observable<ExamResult> {
        return this.http.get<ExamResult>("/api/AuditExam/ResultForUser", { params: { version: version, user: user }, withCredentials: true });
    }
}
