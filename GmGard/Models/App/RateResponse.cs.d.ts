declare module server {
	interface RateResponse {
		status: string;
		message: string;
		rating: {
			blogId: number;
			total: number;
			count: number;
			average: number;
		};
	}
}
