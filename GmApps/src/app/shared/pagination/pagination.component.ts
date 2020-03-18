import { Component, OnInit, OnChanges, Input, Output, EventEmitter } from "@angular/core";

import { Paged } from "app/models/Paged";

@Component({
  selector: "pagination",
  templateUrl: "./pagination.component.html",
  styleUrls: ["./pagination.component.css"]
})
export class PaginationComponent implements OnChanges {

    @Input()
    public paged: Paged<any>;

    @Output()
    public pageChange = new EventEmitter<number>();

    constructor() { }

    ngOnChanges() {
    }

    jump(page: number) {
        if (page >= 1 && page <= this.paged.pageCount) {
            this.pageChange.emit(page);
        }
    }

}
