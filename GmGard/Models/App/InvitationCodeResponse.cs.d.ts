declare module server {
	interface InvitationCodeResponse {
		user: {
			userId: number;
			userName: string;
			nickName: string;
			points: number;
			experience: number;
			level: number;
			avatar: string;
			comment: string;
			roles: string[];
		};
		codes: any[];
		invitedBy: {
			userId: number;
			userName: string;
			nickName: string;
			points: number;
			experience: number;
			level: number;
			avatar: string;
			comment: string;
			roles: string[];
		};
	}
	interface CodeDetail {
		code: string;
		usedBy: {
			userId: number;
			userName: string;
			nickName: string;
			points: number;
			experience: number;
			level: number;
			avatar: string;
			comment: string;
			roles: string[];
		};
	}
}
