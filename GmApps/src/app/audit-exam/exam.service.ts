import { Observable } from "rxjs/Observable";

import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { ENVIRONMENT, Environment } from "../../environments/environment_token";
import { Exam } from "./exam";
import { ExamSubmission } from "../models/ExamSubmission";
import { ExamResult } from "../models/ExamResult";

@Injectable()
export class ExamService {

    private host: string;

    constructor(private http: HttpClient, @Inject(ENVIRONMENT) env: Environment) {
        this.host = env.apiHost;
    }

    currentExamVersions(): string[] {
        return ["201705", "201707"];
    }

    getExam(version: string): Observable<Exam> {
        return this.http.get<Exam>(`/assets/audit-exam-${version}.json`);
    }

    saveExamDraft(draft: ExamSubmission): Observable<boolean> {
        return this.http.put(this.host + "/api/AuditExam/Draft", draft, { withCredentials: true, observe: "response" })
            .map(resp => resp.ok);
    }

    getExamDraft(version: string): Observable<ExamSubmission> {
        return this.http.get<ExamSubmission>(this.host + "/api/AuditExam/Draft", { params: { version: version }, withCredentials: true });
    }

    submitAnswer(submission: ExamSubmission): Observable<ExamResult> {
        return this.http.post<ExamResult>(this.host + "/api/AuditExam/Submit", submission, { withCredentials: true });
    }

    getExamResult(version: string): Observable<ExamResult|null> {
        return this.http.get<ExamResult>(this.host + "/api/AuditExam/Result", { params: { version: version }, withCredentials: true });
    }

    getAllExamResult(): Observable<ExamResult[] | null> {
        return this.http.get<ExamResult[]>(this.host + "/api/AuditExam/Results", { withCredentials: true });
    }

    // Admin Only.
    getExamResultForUser(user: string, version: string): Observable<ExamResult> {
        return this.http.get<ExamResult>(this.host + "/api/AuditExam/ResultForUser", { params: { version: version, user: user }, withCredentials: true });
    }
}
