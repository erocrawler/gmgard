import { NgModule, Injectable } from "@angular/core"
import { MatButtonModule } from "@angular/material/button";
import { MatButtonToggleModule } from "@angular/material/button-toggle";
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatExpansionModule } from "@angular/material/expansion";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatListModule } from "@angular/material/list";
import { MatMenuModule } from "@angular/material/menu";
import { MatPaginatorModule, MatPaginatorIntl } from "@angular/material/paginator";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatSnackBarModule } from "@angular/material/snack-bar";
import { MatTabsModule } from "@angular/material/tabs";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatTooltipModule } from "@angular/material/tooltip";

const modules: any[] = [
    MatSnackBarModule,
    MatButtonModule,
    MatListModule,
    MatCardModule,
    MatInputModule,
    MatMenuModule,
    MatIconModule,
    MatTooltipModule,
    MatCheckboxModule,
    MatToolbarModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatButtonToggleModule,
    MatTabsModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatExpansionModule,
    MatPaginatorModule,
]

@Injectable()
export class ZhCnPaginatorIntl extends MatPaginatorIntl {
    itemsPerPageLabel = '每页个数';
    nextPageLabel = '下一页';
    previousPageLabel = '上一页';

    getRangeLabel = function (page, pageSize, length) {
        if (length === 0 || pageSize === 0) {
            return '没有内容';
        }
        length = Math.max(length, 0);
        const startIndex = page * pageSize;
        // If the start index exceeds the list length, do not try and fix the end index to the end.
        const endIndex = startIndex < length ?
            Math.min(startIndex + pageSize, length) :
            startIndex + pageSize;
        return startIndex + 1 + ' - ' + endIndex + ' 共 ' + length + ' 个';
    };
}

@NgModule({
    imports: modules,
    exports: modules,
    providers: [{
        provide: MatPaginatorIntl,
        useClass: ZhCnPaginatorIntl,
    }],
})
export class AppMaterialModule { }
