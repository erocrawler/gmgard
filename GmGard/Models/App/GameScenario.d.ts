declare module server {
	interface GameStatus {
		progress: number;
		retryCount: number;
		newGameScenarioId: number;
		currentScenario: server.GameScenario;
	}
	interface GameScenario {
		id: number;
		narrators: server.Narrator[];
		dialogs: server.Dialog[];
		next: server.Choice[];
	}
	interface Choice {
		text: string;
		result: number;
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
