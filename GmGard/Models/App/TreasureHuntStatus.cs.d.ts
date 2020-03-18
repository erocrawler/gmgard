declare module server {
	interface TreasureHuntPuzzle {
		position: number;
		image: string;
		hint: string;
		answer: string;
		attempts: { [index: string]: string };
		completed: boolean;
	}
	interface Player {
		avatar: string;
		userId: number;
		userName: string;
		nickName: string;
		completionTime?: Date;
		reward: number;
	}
	interface TreasureHuntStatus {
		topPlayers: server.Player[];
		completedUserCount: number;
		currentPlayer: server.Player;
		puzzles: server.TreasureHuntPuzzle[];
		dailyAttemptLimit: number;
		endTime: Date;
	}
	interface TreasureHuntAttemptRequest {
		id: number;
		answer: string;
	}
	interface TreasureHuntAttemptResult {
		isCorrect: boolean;
		dailyAttemptCount: number;
		reward: number;
		rank: number;
		finalRank?: number;
	}
}
