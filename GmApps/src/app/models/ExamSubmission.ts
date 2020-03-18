export class QuestionSubmission {
	questionId: number;
	answer: string;
}
export class ExamSubmission {
    examAnswers: QuestionSubmission[];
	examVersion: string;
}
