import { Component, OnInit } from "@angular/core";

import { AuthService } from "app/auth/auth.service";
import { Toolbar } from "./toolbar";

@Component({
    selector: "audit-exam-toolbar",
    templateUrl: "./audit-exam-toolbar.component.html",
    styleUrls: ["./toolbar.component.css"]
})
export class AuditExamToolbarComponent extends Toolbar implements OnInit {

    constructor(private auth: AuthService) {
        super()
    }

    showAdmin = false;

    ngOnInit() {
        this.auth.getUser().subscribe(u => this.showAdmin = u.isAdmin());
    }
}
