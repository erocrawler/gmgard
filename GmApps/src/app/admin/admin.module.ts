import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminComponent } from './admin.component';
import { Routes, RouterModule } from '@angular/router';
import { RegistrationComponent } from './registration/registration.component';
import { AppMaterialModule } from '../app-material.module';
import { FormsModule } from '@angular/forms';
import { DeleteConfirmationComponent } from './registration/delete-confirmation.component';
import { MatDialogModule } from '@angular/material/dialog';
import { FlexLayoutModule } from '@angular/flex-layout';
import { RaffleComponent } from './raffle/raffle.component';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { DraftResultComponent } from './raffle/draft-result.component';
import { MatTableModule } from '@angular/material/table';

const routes: Routes = [
  {
    path: "",
    component: AdminComponent,
    children: [
      { path: "", pathMatch: "full", redirectTo: "registration" },
      { path: "registration", component: RegistrationComponent },
      { path: "raffle", component: RaffleComponent },
    ]
  },
];

@NgModule({
  imports: [
    AppMaterialModule,
    MatDialogModule,
    MatDatepickerModule,
    MatMomentDateModule,
    MatTableModule,
    CommonModule,
    FlexLayoutModule,
    FormsModule,
    RouterModule.forChild(routes)
  ],
  entryComponents: [DeleteConfirmationComponent],
  declarations: [AdminComponent, RegistrationComponent, DeleteConfirmationComponent, RaffleComponent, DraftResultComponent]
})
export class AdminModule { }
