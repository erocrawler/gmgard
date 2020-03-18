export interface GachaItem {
    name: string;
    rarity: number;
}
export interface GachaResult {
    success: boolean;
    errorMessage: string;
    currentPoint: number;
    rewards: string[];
    items: GachaItem[];
}
export interface GachaItemDetails {
    name: string;
    title: string;
    rarity: number;
    description: string;
    itemCount: number;
}
export interface GachaStats {
    totalCards: number;
	progresses: string[];
    userCards: GachaItemDetails[];
}
