export interface TreasureHuntPuzzle {
	position: number;
	image: string;
	hint: string;
	answer: string;
  attempts: { [index: string]: string };
  completed: boolean;
}
export interface Player {
	avatar: string;
	userId: number;
	userName: string;
	nickName: string;
	completionTime?: Date;
	reward: number;
}
export interface TreasureHuntStatus {
  topPlayers: Player[];
	completedUserCount: number;
	currentPlayer: Player;
  puzzles: TreasureHuntPuzzle[];
	dailyAttemptLimit: number;
	endTime: Date;
}
export interface TreasureHuntAttemptResult {
	isCorrect: boolean;
	dailyAttemptCount: number;
	reward: number;
	rank: number;
}
