declare module server {
	interface BlogPreview {
		id: number;
		url: string;
		title: string;
		brief: string;
		thumbUrl: string;
		createDate: Date;
	}
}
