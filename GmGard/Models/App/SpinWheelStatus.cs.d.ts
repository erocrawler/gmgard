declare module server {
	interface spinWheelStatus {
		title: string;
		isActive: boolean;
		userPoints: number;
		wheelACost: number;
		wheelADailyLimit: number;
		wheelBCost: number;
		ceilingCost: number;
		showRedeem: boolean;
		vouchers: server.vouchers[];
		wheelAPrizes: any[];
		wheelBPrizes: any[];
		displayPrizes: any[];
		couponPrizes: any[];
	}
}
