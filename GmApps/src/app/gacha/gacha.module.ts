import { NgModule } from "@angular/core";
import { RouterModule, Router, Routes } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { FlexLayoutModule } from "@angular/flex-layout";
import { MatDialogModule } from "@angular/material/dialog";
import { MatGridListModule } from "@angular/material/grid-list";
import { GachaComponent } from "./gacha.component";
import { GachaIndexComponent, GachaConfirmComponent } from "./gacha-index.component";
import { GachaResultComponent } from "./gacha-result.component";
import { dbConfig, GachaService } from "./gacha.service";
import { AppMaterialModule } from "../app-material.module";
import { CardDetailComponent } from "./card-detail.component";
import { GachaListComponent } from "./gacha-list.component";
import { GachaIntroComponent } from './gacha-intro.component';
import { AdvGameModule } from "../shared/adv-game/adv-game.module";
import { NgxIndexedDBModule } from "ngx-indexed-db";

const routes: Routes = [
  {
    path: "",
    component: GachaComponent,
    children: [
      { path: "", component: GachaIndexComponent, pathMatch: "full" },
      { path: "result", component: GachaResultComponent },
      { path: "list", component: GachaListComponent },
      { path: "intro", component: GachaIntroComponent },
    ]
  },
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    AppMaterialModule,
    MatDialogModule,
    MatGridListModule,
    FlexLayoutModule,
    RouterModule.forChild(routes),
    NgxIndexedDBModule.forRoot(dbConfig),
    AdvGameModule,
  ],
  declarations: [
    GachaComponent,
    GachaIndexComponent,
    GachaResultComponent,
    GachaConfirmComponent,
    CardDetailComponent,
    GachaListComponent,
    GachaIntroComponent
  ],
  entryComponents: [GachaConfirmComponent, CardDetailComponent],
  providers: [GachaService],
})
export class GachaModule { }
