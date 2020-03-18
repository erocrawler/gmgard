import { IUser } from "./User";

export interface GetInvitationCodeRequest {
    userName?: string;
    code?: string;
}

export interface InvitationCodeResponse {
    user?: IUser;
    codes?: CodeDetail[];
    invitedBy?: IUser;
}
export interface CodeDetail {
    code: string;
    usedBy: IUser;
}
