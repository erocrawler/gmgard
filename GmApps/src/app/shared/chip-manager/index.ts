import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatChipsModule, MatIconModule, MatInputModule, MatButtonModule } from "@angular/material";
import { FlexLayoutModule } from "@angular/flex-layout";
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
