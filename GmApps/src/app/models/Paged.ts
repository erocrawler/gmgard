export class Paged<T> {
    items: T[];
    pageCount: number;
    totalItemCount: number;
    pageNumber: number;
    pageSize: number;
    get hasPreviousPage(): boolean { return this.pageNumber > 1 };
    get hasNextPage(): boolean { return this.pageNumber < this.pageCount };
}
