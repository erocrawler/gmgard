declare module server {
	interface Category {
		id: number;
		name: string;
		parentId?: number;
	}
}
