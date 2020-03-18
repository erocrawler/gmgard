declare module server {
	interface GachaItem {
		name: string;
		rarity: number;
	}
	interface GachaResult {
		success: boolean;
		errorMessage: string;
		currentPoint: number;
		rewards: string[];
		items: any[];
	}
}
