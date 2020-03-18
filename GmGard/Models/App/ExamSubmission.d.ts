declare module server {
	interface QuestionSubmission {
		questionId: number;
		answer: string;
	}
	interface ExamSubmission {
		examAnswers: any[];
		examVersion: string;
	}
}
