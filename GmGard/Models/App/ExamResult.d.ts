declare module server {
	interface ExamAnswer {
		questionId: number;
		answer: string;
		comment: string;
		point: number;
		correct: boolean;
	}
	interface ExamResult {
		examAnswers: any[];
		examVersion: string;
		hasPassed: boolean;
		submitTime: Date;
        isSubmitted: boolean;
        score: number;
	}
}
