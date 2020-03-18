export interface TwoFactorAuthenticationModel {
    hasAuthenticator: boolean;
    recoveryCodesLeft: number;
    is2faEnabled: boolean;
    isMachineRemembered: boolean;
}

export interface TwoFactorAuthSharedKey {
    sharedKey: string;
    authenticatorUri: string;
}
