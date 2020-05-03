declare module server {
	interface spinWheelResult {
		prize: {
			prizeName: string;
			prizePic: string;
			isRealItem: boolean;
			isVoucher: boolean;
			prizeLPValue: number;
			drawPercentage: number;
			redeemItemName: string;
		};
		voucher: server.vouchers;
	}
}
