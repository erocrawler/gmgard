declare module server {
	interface vouchers {
		voucherID: string;
		issueTime: Date;
		useTime?: Date;
		redeemItem: string;
		kind: any;
		userName: string;
	}
}
