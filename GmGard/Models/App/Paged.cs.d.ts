declare module server {
	interface Paged {
		items: any[];
		pageCount: number;
		totalItemCount: number;
		pageNumber: number;
		pageSize: number;
		skip: number;
	}
}
