declare module server {
	interface CommentChangeModel {
		comment: string;
	}
	interface LocalPasswordModel {
		oldPassword: string;
		newPassword: string;
		confirmPassword: string;
	}
	interface LoginModel {
		userName: string;
		password: string;
		rememberMe: boolean;
		captcha: string;
	}
	interface RegisterModel {
		userName: string;
		nickName: string;
		password: string;
		confirmPassword: string;
		email: string;
		avatar: any;
		captcha: string;
		x: number;
		y: number;
		w: number;
		h: number;
		registerQuestionIndex: number;
		registerAnswer: string;
		registerCode: string;
	}
	interface PasswordResetModel {
		email: string;
		code: string;
		password: string;
		confirmPassword: string;
	}
	interface HPSettingsModel {
		categoryOptions: any[];
		hideHarmony: boolean;
		categoryIds: number[];
		blacklistTagNames: string;
	}
	interface CategoryOption {
		categoryId: number;
		showCategory: boolean;
	}
	interface FollowModel {
		userName: string;
		followEachOther: boolean;
		userComment: string;
		experience: number;
	}
}
