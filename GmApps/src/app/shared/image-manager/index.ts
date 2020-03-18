import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ImageManagerComponent } from "./image-manager.component";
import { MatIconModule, MatButtonModule, MatGridListModule } from "@angular/material";

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
