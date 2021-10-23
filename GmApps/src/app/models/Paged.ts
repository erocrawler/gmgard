export interface Paged<T> {
    items: T[];
    pageCount: number;
    totalItemCount: number;
    pageNumber: number;
    pageSize: number;
}
