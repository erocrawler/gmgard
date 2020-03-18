import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { AppMaterialModule } from 'app/app-material.module';
import { TreasureHuntComponent } from './treasure-hunt/treasure-hunt.component';
import { PuzzleDetailComponent } from './treasure-hunt/puzzle-detail.component';
import { MatDialogModule } from '@angular/material';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule } from '@angular/forms';
import { HintDialogComponent } from './treasure-hunt/hint-dialog.component';
import { IndexComponent } from './eternal-circle/index.component';
import { PlayComponent } from './eternal-circle/play.component';
import { AlertModule } from 'app/shared/alert-dialog';

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
      {
        path: "play",
        pathMatch: "full",
        component: PlayComponent,
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
    RouterModule.forChild(routes),
  ],
  entryComponents: [PuzzleDetailComponent, HintDialogComponent],
  declarations: [TreasureHuntComponent, PuzzleDetailComponent, HintDialogComponent, IndexComponent, PlayComponent],
})
export class GameModule { }
