import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ImageManagerComponent } from "./image-manager.component";
import { MatIconModule } from "@angular/material/icon";
import { MatGridListModule } from "@angular/material/grid-list";
import { MatButtonModule } from "@angular/material/button";

@NgModule({
  imports: [
      CommonModule,
      MatIconModule,
      MatButtonModule,
      MatGridListModule
    ],
  exports: [ImageManagerComponent],
  declarations: [ImageManagerComponent]
})
export class ImageManagerModule { }
