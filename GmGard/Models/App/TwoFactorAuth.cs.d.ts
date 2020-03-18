declare module server {
	interface twoFactorAuthenticationModel {
		hasAuthenticator: boolean;
		recoveryCodesLeft: number;
		is2faEnabled: boolean;
		isMachineRemembered: boolean;
	}
	interface twoFactorAuthSharedKey {
		sharedKey: string;
		authenticatorUri: string;
	}
}
