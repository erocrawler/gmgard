import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { AppMaterialModule } from 'app/app-material.module';
import { TreasureHuntComponent } from './treasure-hunt/treasure-hunt.component';
import { PuzzleDetailComponent } from './treasure-hunt/puzzle-detail.component';
import { MatDialogModule } from '@angular/material/dialog';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule } from '@angular/forms';
import { HintDialogComponent } from './treasure-hunt/hint-dialog.component';
import { IndexComponent as EcIndexComponent } from './eternal-circle/index.component';
import { IndexComponent as TwIndexComponent } from './tarnished-world/index.component';
import { AlertModule } from 'app/shared/alert-dialog';
import { AdvGameModule } from '../shared/adv-game/adv-game.module';
import { MatMenuModule } from '@angular/material/menu';
import { ItemListComponent } from './tarnished-world/item-list.component';
import { MatGridListModule } from '@angular/material/grid-list';

const routes: Routes = [
  {
    path: "treasure-hunt",
    pathMatch: "full",
    component: TreasureHuntComponent,
  },
  {
    path: "eternal-circle",
    children: [
      {
        path: "",
        pathMatch: "full",
        component: EcIndexComponent,
      },
    ]
  },
  {
    path: "tarnished-world",
    children: [
      {
        path: "",
        pathMatch: "full",
        component: TwIndexComponent,
      },
    ]
  },
];

@NgModule({
    imports: [
        AppMaterialModule,
        MatDialogModule,
        MatMenuModule,
        MatGridListModule,
        CommonModule,
        FlexLayoutModule,
        FormsModule,
        AlertModule,
        AdvGameModule,
        RouterModule.forChild(routes),
    ],
  declarations: [TreasureHuntComponent, PuzzleDetailComponent, HintDialogComponent, EcIndexComponent, TwIndexComponent, ItemListComponent]
})
export class GameModule { }
