import { Component, OnInit } from '@angular/core';
import { AdminService, Category } from '../admin.service';

interface CategoryView extends Category {
  editing: boolean
  options: string[]
  parentName: string
  saving?: boolean
}

@Component({
  selector: 'app-category',
  templateUrl: './category.component.html',
  styleUrls: ['./category.component.css']
})
export class CategoryComponent implements OnInit {

  constructor(private service: AdminService) { }

  categories: Map<number, Category>
  categoryViewModels: CategoryView[] = [];
  categoryDropdown: {value: number, viewValue: string}[]

  private toOptionList(c: Category): string[] {
    const ret: string[] = [];
    for (const k in c) {
      if (typeof c[k] === "boolean" && c[k]) {
        ret.push(k);
      }
    }
    return ret;
  }

  private updateCategoryDropdown() {
    const catStack: { c: Category, depth: number }[] =
      this.categoryViewModels
        .filter(v => !v.parentCategoryID)
        .sort((a, b) => b.categoryID - a.categoryID)
        .map(v => ({ c: v, depth: 0 }));
    const subCats = new Map<number, Category[]>();
    for (const cat of this.categories.values()) {
      if (cat.parentCategoryID) {
        let cs = subCats.get(cat.parentCategoryID) || [];
        cs.push(cat);
        subCats.set(cat.parentCategoryID, cs);
      }
    }
    const categoryDropdown: { value: number, viewValue: string }[] = []
    while (catStack.length > 0) {
      const cat = catStack.pop();
      const item = {
        value: cat.c.categoryID,
        viewValue: cat.c.categoryName
      };
      if (cat.depth > 0) {
        item.viewValue = "└".padStart(cat.depth, '　') + item.viewValue;
      }
      categoryDropdown.push(item);
      if (subCats.has(cat.c.categoryID)) {
        for (const subcat of subCats.get(cat.c.categoryID)) {
          catStack.push({ c: subcat, depth: cat.depth + 1 });
        }
      }
    }
    this.categoryDropdown = categoryDropdown;
  }

  private toViewModel(c: Category): CategoryView {
    return {
      categoryID: c.categoryID,
      categoryName: c.categoryName,
      parentCategoryID: c.parentCategoryID,
      description: c.description,
      disableRanking: c.disableRanking,
      disableRating: c.disableRating,
      hideFromHomePage: c.hideFromHomePage,
      linkOptional: c.linkOptional,
      editing: false,
      options: this.toOptionList(c),
      parentName: c.parentCategoryID ? this.categories.get(c.parentCategoryID).categoryName : '',
    }
  }

  ngOnInit(): void {
    this.service.getCategories().subscribe(cats => {
      this.categories = new Map<number, Category>();
      for (let c of cats) {
        this.categories.set(c.categoryID, c);
      }
      this.categoryViewModels = cats.map((c) => this.toViewModel(c));
      this.updateCategoryDropdown();
    });
  }

  add() {
    const newCat: CategoryView = {
      categoryID: -1,
      categoryName: '',
      description: '',
      disableRanking: false,
      disableRating: false,
      hideFromHomePage: false,
      linkOptional: false,
      editing: true,
      options: [],
      parentName: '',
    }
    this.categoryViewModels.push(newCat);
  }

  save(cat: CategoryView) {
    cat.saving = true;
    const newCat: Category = {
      categoryID: cat.categoryID,
      categoryName: cat.categoryName,
      description: cat.description,
      disableRanking: cat.options.indexOf("disableRanking") >= 0,
      disableRating: cat.options.indexOf("disableRating") >= 0,
      hideFromHomePage: cat.options.indexOf("hideFromHomePage") >= 0,
      linkOptional: cat.options.indexOf("linkOptional") >= 0,
      parentCategoryID: cat.parentCategoryID,
    }
    this.service.updateCategory(newCat).subscribe(id => {
      newCat.categoryID = id;
      this.categories.set(id, newCat);
      this.cancelEdit(cat);
      cat.saving = false;
      this.updateCategoryDropdown();
    })
  }

  delete(cat: CategoryView) {
    let doRemove = () => {
      const idx = this.categoryViewModels.indexOf(cat);
      if (idx >= 0) {
        this.categoryViewModels.splice(idx, 1);
      }
    }
    if (this.categories.has(cat.categoryID)) {
      cat.saving = true;
      this.service.deleteCategory(cat.categoryID).subscribe(_ => {
        this.categories.delete(cat.categoryID);
        doRemove();
        this.updateCategoryDropdown();
      })
    } else {
      doRemove();
    }
  }

  cancelEdit(cat: CategoryView) {
    if (this.categories.has(cat.categoryID)) {
      Object.assign(cat, this.toViewModel(this.categories.get(cat.categoryID)));
      cat.editing = false;
    } else {
      this.delete(cat);
    }
  }

  edit(cat: CategoryView) {
    cat.editing = true;
  }
}
