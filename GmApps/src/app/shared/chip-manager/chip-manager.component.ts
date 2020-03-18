import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";

@Component({
  selector: "chip-manager",
  templateUrl: "./chip-manager.component.html",
  styleUrls: ["./chip-manager.component.css"]
})
export class ChipManagerComponent implements OnInit {

    @Input()
    chips: string[];

    @Output()
    chipsChange = new EventEmitter<string[]>();

    @Input()
    max: number;

    maxHint: string;

    constructor() { }

    ngOnInit() {
        if (this.max) {
            this.maxHint = "，最多" + this.max + "个。";
        }
    }

    add(chip: string) {
        if (this.chips.indexOf(chip) >= 0 || this.chips.length == this.max) {
            return;
        }
        this.chips.push(chip);
        this.chipsChange.emit(this.chips);
    }

    remove(chip: string) {
        const tmp = this.chips.filter(c => c != chip);
        this.chipsChange.emit(tmp);
    }

    oninput(t: HTMLInputElement) {
        const val = t.value;
        const spacei = val.indexOf(" ");
        if (spacei < 0) {
            return;
        }
        t.value = "";
        if (spacei == 0) {
            return;
        }
        if (spacei == val.length - 1) {
            this.add(val.substring(0, spacei));
            return;
        }
        const arr = val.split(" ");
        if (arr[arr.length - 1] != "") {
            t.value = arr.pop();
        }
        arr.forEach(s => {
            if (s) {
                this.chips.push(s);
            }
        });
        this.chipsChange.emit(this.chips);
    }

}
