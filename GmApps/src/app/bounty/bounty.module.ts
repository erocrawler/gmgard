import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule } from "@angular/forms";

import { CKEditorModule } from "ng2-ckeditor";
import { Ng2PageScrollModule, PageScrollService } from "ng2-page-scroll";
import { CkeditorResolverService } from "./ckeditor-resolver.service";

import { AppMaterialModule } from "../app-material.module";
import { ChipManagerModule } from "../shared/chip-manager";
import { PaginationModule } from "../shared/pagination";
import { ImageManagerModule } from "../shared/image-manager";
import { FileSizePipeModule } from "../shared/file-size-pipe";
import { BountyComponent } from "./bounty.component";
import { BountyService } from "./bounty.service";
import { BountyListComponent } from "./bounty-list.component";
import { BountyRoutingModule } from "./bounty-routing.module";
import { AskComponent } from "./ask/ask.component";

@NgModule({
    imports: [
        FileSizePipeModule,
        CommonModule,
        ChipManagerModule,
        ImageManagerModule,
        PaginationModule,
        AppMaterialModule,
        BountyRoutingModule,
        FlexLayoutModule,
        FormsModule,
        CKEditorModule,
        Ng2PageScrollModule.forRoot(),
    ],
    declarations: [
        BountyComponent,
        BountyListComponent,
        AskComponent,
    ],
    providers: [
        BountyService,
        PageScrollService,
    ]
})
export class BountyModule { }
