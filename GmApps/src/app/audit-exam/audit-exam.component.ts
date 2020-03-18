import { Component, OnInit } from "@angular/core";

@Component({
    template: "<router-outlet></router-outlet>",
    styles: [":host {width: 100%; max-width:1000px}"],
})
export class AuditExamComponent implements OnInit {

    constructor() {
    }

    ngOnInit() {
    }
}
