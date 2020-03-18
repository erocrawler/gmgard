import { NgModule } from "@angular/core";
import { RouterModule, Router, Routes } from "@angular/router";

import { BountyComponent } from "./bounty.component";
import { BountyListComponent } from "./bounty-list.component";
import { AskComponent } from "./ask/ask.component";
import { CkeditorResolverService } from "./ckeditor-resolver.service";

const routes: Routes = [
    {
        path: "",
        component: BountyComponent,
        children: [
            { path: "", component: BountyListComponent, pathMatch: "full" },
            {
                path: "ask",
                component: AskComponent,
                resolve: {
                    ckeditorLoaded: CkeditorResolverService
                }
            },
        ]
    },

];
@NgModule({
    imports: [
        RouterModule.forChild(routes)
    ],
    exports: [RouterModule],
    providers: [
        CkeditorResolverService
    ],
})
export class BountyRoutingModule { }
