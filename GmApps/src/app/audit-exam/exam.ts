
export class Exam {
    version: string;
    questions: Question[];
}

export enum QuestionType {
    Information,
    MultipleChoices,
    TitleCombination,
    FillInBlank,
}

export class Question {
    id: number;
    description: string;
    choices: string[];
    type: string;
}
