import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { AppMaterialModule } from "../../app-material.module";
import { FlexLayoutModule } from "@angular/flex-layout";
import { PaginationComponent } from "./pagination.component";

@NgModule({
  imports: [
      CommonModule,
      AppMaterialModule,
      FlexLayoutModule,
    ],
  exports: [
      PaginationComponent,
  ],
  declarations: [PaginationComponent]
})
export class PaginationModule { }

export { PaginationComponent } from "./pagination.component";
