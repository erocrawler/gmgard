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
import { IndexComponent } from './eternal-circle/index.component';
import { AlertModule } from 'app/shared/alert-dialog';
import { AdvGameModule } from '../shared/adv-game/adv-game.module';

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
        component: IndexComponent,
      },
    ]
  },
];

@NgModule({
  imports: [
    AppMaterialModule,
    MatDialogModule,
    CommonModule,
    FlexLayoutModule,
    FormsModule,
    AlertModule,
    AdvGameModule,
    RouterModule.forChild(routes),
  ],
  entryComponents: [PuzzleDetailComponent, HintDialogComponent],
  declarations: [TreasureHuntComponent, PuzzleDetailComponent, HintDialogComponent, IndexComponent],
})
export class GameModule { }
