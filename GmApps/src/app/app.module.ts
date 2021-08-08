import { BrowserModule } from "@angular/platform-browser";
import { NgModule, LOCALE_ID } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { registerLocaleData } from "@angular/common";
import { HttpClientModule, HttpClientXsrfModule } from "@angular/common/http";
import { FlexLayoutModule } from "@angular/flex-layout";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { ReactiveFormsModule } from "@angular/forms";
import localeCn from '@angular/common/locales/zh';

import { ClipboardModule } from "ngx-clipboard";
import { AppMaterialModule } from "./app-material.module";

import { environment } from "../environments/environment";
import { ENVIRONMENT } from "../environments/environment_token";

import { AppComponent } from "./app.component";
import { AppRoutingModule } from "./app-routing.module";
import { TitleHelperComponent } from "./title-helper/title-helper.component";
import { CategoryFieldsComponent } from "./title-helper/category-fields.component";
import { PageNotFoundComponent } from "./page-not-found.component"
import { TitleSearchComponent } from "./title-helper/title-search.component";
import { LoginComponent } from "./login/login.component";
import { DlsiteSearchComponent } from "./title-helper/dlsite-search.component";
import { AuditExamToolbarComponent } from "./toolbar/audit-exam-toolbar.component";
import { RaffleIndexComponent } from "./raffle/raffle-index.component";
import { AppLayoutComponent } from './app-layout.component';

registerLocaleData(localeCn, "zh-CN");

@NgModule({
  declarations: [
    AppComponent,
    TitleHelperComponent,
    CategoryFieldsComponent,
    PageNotFoundComponent,
    TitleSearchComponent,
    LoginComponent,
    DlsiteSearchComponent,
    AuditExamToolbarComponent,
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
  ],
  entryComponents: [AuditExamToolbarComponent],
  bootstrap: [AppComponent]
})
export class AppModule { }
