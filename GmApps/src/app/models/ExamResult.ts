export class ExamAnswer {
	questionId: number;
	answer: string;
	comment: string;
	point: number;
}

export class ExamResult {
    examAnswers: ExamAnswer[];
	examVersion: string;
    hasPassed: boolean;
    submitTime: Date;
    isSubmitted: boolean;
    score: number;
}
