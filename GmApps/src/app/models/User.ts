export interface IUser {
  userId: number;
  userName: string;
  nickName: string;
  points: number;
  experience: number;
  level: number;
  avatar: string;
  roles: string[];
  consecutiveSign: number;
}

export class User implements IUser {
  userId: number;
  userName: string;
  nickName: string;
  points: number;
  experience: number;
  level: number;
  avatar: string;
  roles: string[];
  consecutiveSign: number;

  constructor(i: IUser) {
    this.userId = i.userId;
    this.userName = i.userName;
    this.nickName = i.nickName;
    this.points = i.points;
    this.experience = i.experience;
    this.level = i.level;
    this.avatar = i.avatar;
    this.roles = i.roles;
    this.consecutiveSign = i.consecutiveSign;
  }

  isInRole(role: string) {
    return this.roles.indexOf(role) >= 0;
  }

  isAdmin() {
    return this.isInRole("Administrator") || this.isInRole("Moderator");
  }

  isAdmanager() {
    return this.isInRole("AdManager") || this.isInRole("Administrator")
  }
}
