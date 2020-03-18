import { InjectionToken } from "@angular/core";

export interface Environment {
  production: boolean,
  apiHost: string,
}
export const ENVIRONMENT = new InjectionToken<Environment>("Environment");
