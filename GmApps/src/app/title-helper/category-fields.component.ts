import { Subscription } from "rxjs"
import { Component, Input, OnChanges, EventEmitter, Output } from "@angular/core";
import { UntypedFormControl, UntypedFormGroup, Validators } from "@angular/forms";

import { Category } from "./category";

@Component({
    selector: "category-fields",
    templateUrl: "./category-fields.component.html",
    styleUrls: ["./category-fields.component.css"],
})
export class CategoryFieldsComponent implements OnChanges  {
    @Input()
    category: Category;

    categoryForm: UntypedFormGroup;

    @Output()
    title = new EventEmitter<string>();

    private titleSubscription: Subscription;

    constructor() {
    }

    onSubmit() { }

    createForm() {
        if (this.titleSubscription) {
            this.titleSubscription.unsubscribe();
        }
        const group = {};
        group["前缀"] = new UntypedFormControl();
        this.category.fields.forEach(cf => {
            group[cf.name] = new UntypedFormControl(
                cf.default,
                cf.required ? Validators.required : Validators.nullValidator)
        });
        group["后缀"] = new UntypedFormControl();
        this.categoryForm = new UntypedFormGroup(group);


        this.titleSubscription = this.categoryForm.valueChanges
            .subscribe(v => { this.title.emit(this.formatTitle()) });
    }

    formatTitle(): string {
        const pieces: string[] = [];
        const prefix = this.categoryForm.value["前缀"],
            suffix = this.categoryForm.value["后缀"];
        if (prefix) {
            pieces.push(prefix);
        }
        this.category.fields.forEach(cf => {
            const val = this.categoryForm.value[cf.name];
            if (val) {
                pieces.push(cf.format ? cf.format(val) : val);
            }
        });
        if (suffix) {
            pieces.push(suffix);
        }
        return pieces.join("");
    }

    ngOnInit() {
    }

    ngOnChanges() {
        this.createForm();
    }
}
