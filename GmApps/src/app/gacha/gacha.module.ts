import { NgModule } from "@angular/core";
import { RouterModule, Router, Routes } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { FlexLayoutModule } from "@angular/flex-layout";
import { MatDialogModule, MatGridListModule } from "@angular/material";
import { GachaComponent } from "./gacha.component";
import { GachaIndexComponent, GachaConfirmComponent } from "./gacha-index.component";
import { GachaResultComponent } from "./gacha-result.component";
import { GachaService } from "./gacha.service";
import { AppMaterialModule } from "../app-material.module";
import { CardDetailComponent } from "./card-detail.component";
import { GachaListComponent } from "./gacha-list.component";

const routes: Routes = [
    {
        path: "",
        component: GachaComponent,
        children: [
            { path: "", component: GachaIndexComponent, pathMatch: "full" },
            { path: "result", component: GachaResultComponent },
            { path: "list", component: GachaListComponent },
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
    ],
    declarations: [
        GachaComponent,
        GachaIndexComponent,
        GachaResultComponent,
        GachaConfirmComponent,
        CardDetailComponent,
        GachaListComponent
    ],
    entryComponents: [GachaConfirmComponent, CardDetailComponent],
    providers: [GachaService],
})
export class GachaModule { }
