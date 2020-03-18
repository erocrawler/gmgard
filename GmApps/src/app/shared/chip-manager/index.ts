import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatChipsModule } from "@angular/material/chips";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatButtonModule } from "@angular/material/button";
import { ChipManagerComponent } from "./chip-manager.component";

@NgModule({
    imports: [
        CommonModule,
        MatChipsModule,
        MatIconModule,
        MatInputModule,
        MatButtonModule,
    ],
    exports: [
        ChipManagerComponent,
    ],
    declarations: [ChipManagerComponent]
})
export class ChipManagerModule { }

export { ChipManagerComponent } from "./chip-manager.component";
