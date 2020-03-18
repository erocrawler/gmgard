declare module server {
	interface User {
		userId: number;
		userName: string;
		nickName: string;
		points: number;
		experience: number;
		level: number;
		avatar: string;
		comment: string;
		roles: string[];
	}
}
