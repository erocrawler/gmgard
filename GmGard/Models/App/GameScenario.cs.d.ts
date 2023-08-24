declare module server {
	interface GameStatus {
		progress: number;
		retryCount: number;
		newGameScenarioId: number;
		inventory: server.GameInventory[];
		currentScenario: server.GameScenario;
		chapters: server.GameChapter[];
	}
	interface GameChapter {
		id: number;
		name: string;
	}
	interface GameInventory {
		name: string;
		description: string;
	}
	interface GameScenario {
		id: number;
		narrators: server.Narrator[];
		dialogs: server.Dialog[];
		next: server.Choice[];
		data: any;
	}
	interface Choice {
		text: string;
		result: number;
		locked: boolean;
	}
	interface ChoiceData {
		getItems: string[];
		requireItems: string[];
		getTitles: string[];
		questionResult: server.QuestionResult;
	}
	interface ScoreResult {
		score: number[];
		next: number;
	}
	interface QuestionResult {
		answers: number[];
		results: server.ScoreResult[];
	}
	interface Narrator {
		avatar: string;
		name: string;
		display: string;
	}
	interface Dialog {
		bgImg: string;
		texts: any[];
		effect: server.DialogEffect[];
	}
	interface DialogEffect {
		pos: number;
		kind: server.Effects;
	}
	const enum Effects {
		NONE,
		SPARK,
		SHAKE,
		FLASH,
		TITLE,
		CENTER_BG,
	}
}
