import { NgModule } from "@angular/core"
import {
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
    MatPaginatorIntl,
} from "@angular/material";

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
