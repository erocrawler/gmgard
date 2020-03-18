declare module server {
	interface ByCategory {
		id: number;
		count: number;
	}
	interface NewItemCount {
		since: Date;
		total: number;
		byCategories: server.ByCategory[];
	}
}
