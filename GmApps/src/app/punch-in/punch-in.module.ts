import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FlexLayoutModule } from "@angular/flex-layout";
import { PunchInComponent } from './punch-in.component';
import {
  MatDatepickerModule, MatProgressBarModule, MatCardModule, MatDialogModule,
  MatSnackBarModule, DateAdapter, MatButtonModule, MatProgressSpinnerModule
} from '@angular/material';
import { MatMomentDateModule, MomentDateAdapter } from '@angular/material-moment-adapter';
import { PunchInDetailComponent } from './punch-in-detail.component';
import { PunchInService } from './punch-in.service';
import { UserResolverService } from 'app/auth/user-resolver.service';

const routes: Routes = [
  {
    path: "",
    component: PunchInComponent,
    resolve: {
      user: UserResolverService
    }
  },
];

export class CustomDateAdapter extends MomentDateAdapter {
  getDateNames(): string[] {
    let names : string[] = [];
    for (let i = 1; i <= 31; i++) {
      names.push(i.toString());
    }
    return names;
  }
}

@NgModule({
  declarations: [PunchInComponent, PunchInDetailComponent],
  entryComponents: [PunchInComponent, PunchInDetailComponent],
  imports: [
    CommonModule,
    FlexLayoutModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatProgressBarModule,
    MatCardModule,
    MatDialogModule,
    MatButtonModule,
    MatMomentDateModule,
    MatSnackBarModule,
    RouterModule.forChild(routes),
  ],
  providers: [
    PunchInService,
    { provide: DateAdapter, useClass: CustomDateAdapter },
  ],
})
export class PunchInModule { }
