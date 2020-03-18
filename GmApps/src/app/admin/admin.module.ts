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

const routes: Routes = [
    {
        path: "",
        component: AdminComponent,
        children: [
            { path: "", pathMatch: "full", redirectTo: "registration" },
            { path: "registration", component: RegistrationComponent },
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
        RouterModule.forChild(routes)
    ],
    entryComponents: [DeleteConfirmationComponent],
    declarations: [AdminComponent, RegistrationComponent, DeleteConfirmationComponent]
})
export class AdminModule { }
