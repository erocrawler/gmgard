import { BrowserModule } from "@angular/platform-browser";
import { NgModule, LOCALE_ID, Injectable, Inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { registerLocaleData } from "@angular/common";
import { HttpClientModule, HttpClientXsrfModule, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HTTP_INTERCEPTORS } from "@angular/common/http";
import { FlexLayoutModule } from "@angular/flex-layout";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { ReactiveFormsModule } from "@angular/forms";
import localeCn from '@angular/common/locales/zh';

import { ClipboardModule } from "ngx-clipboard";
import { AppMaterialModule } from "./app-material.module";

import { environment } from "../environments/environment";
import { Environment, ENVIRONMENT } from "../environments/environment_token";

import { AppComponent } from "./app.component";
import { AppRoutingModule } from "./app-routing.module";
import { TitleHelperComponent } from "./title-helper/title-helper.component";
import { CategoryFieldsComponent } from "./title-helper/category-fields.component";
import { PageNotFoundComponent } from "./page-not-found.component"
import { TitleSearchComponent } from "./title-helper/title-search.component";
import { LoginComponent } from "./login/login.component";
import { DlsiteSearchComponent } from "./title-helper/dlsite-search.component";
import { RaffleIndexComponent } from "./raffle/raffle-index.component";
import { AppLayoutComponent } from './app-layout.component';
import { Observable } from "rxjs";

registerLocaleData(localeCn, "zh-CN");

@Injectable()
export class EnvHostInterceptor implements HttpInterceptor {

  host: string
  constructor(@Inject(ENVIRONMENT) env: Environment) {
    this.host = env.apiHost;
  }

  intercept(req: HttpRequest<any>, next: HttpHandler):
    Observable<HttpEvent<any>> {
    return next.handle(req.clone({url: this.host + req.url, withCredentials: true}));
  }
}

@NgModule({
  declarations: [
    AppComponent,
    TitleHelperComponent,
    CategoryFieldsComponent,
    PageNotFoundComponent,
    TitleSearchComponent,
    LoginComponent,
    DlsiteSearchComponent,
    RaffleIndexComponent,
    AppLayoutComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    HttpClientXsrfModule.disable(),
    FlexLayoutModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    ClipboardModule,
    AppMaterialModule,
    AppRoutingModule,
  ],
  providers: [
    { provide: ENVIRONMENT, useValue: environment },
    { provide: LOCALE_ID, useValue: "zh-CN" },
    { provide: HTTP_INTERCEPTORS, useClass: EnvHostInterceptor, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
