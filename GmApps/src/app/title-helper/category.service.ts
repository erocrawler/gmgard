import { Injectable } from "@angular/core";

import { Category, CATEGORIES } from "./category";

@Injectable()
export class CategoryService {

    private categories: Category[];

    constructor() { }

    allCategories(): Promise<Category[]> {
        if (this.categories) {
            return Promise.resolve(this.categories);
        }
        return Promise.resolve(CATEGORIES).then(c => this.categories = c);
    }
}
