declare module server {
	interface spinWheelStatus {
		title: string;
		isActive: boolean;
		userPoints: number;
		wheelACost: number;
		wheelBCost: number;
		ceilingCost: number;
		vouchers: server.vouchers[];
		wheelAPrizes: any[];
		wheelBPrizes: any[];
	}
}
