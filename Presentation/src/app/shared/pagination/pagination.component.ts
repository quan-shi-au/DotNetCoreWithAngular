import {Component, OnInit, OnChanges, Input, Output, EventEmitter} from '@angular/core';
import {Observable} from 'rxjs';

@Component({
    moduleId: module.id.toString(),
    selector: 'app-pagination',
    templateUrl: 'pagination.component.html',
    styleUrls: ['pagination.css']
})

export class PaginationComponent implements OnInit, OnChanges {
    @Input() offset: number;
    @Input() limit: number;
    @Input() size: number;
    @Input() range: number = 2;
    @Output() pageChange: EventEmitter<number> = new EventEmitter<number>();

    pages: Observable<number[]>;
    currentPage: number;
    totalPages: number;

    constructor() { }

    ngOnInit() {
        this.getPages(this.offset, this.limit, this.size);
    }

    ngOnChanges() {

        this.getPages(this.offset, this.limit, this.size);
    }

    getPages(offset: number, limit: number, size: number) {

        this.currentPage = this.getCurrentPage(offset, limit);
        this.totalPages = this.getTotalPages(limit, size);
        this.pages = Observable.range(1, this.totalPages)
            .toArray();
    }


    isValidPageNumber(page: number, totalPages: number): boolean {

        return page > 0 && page <= totalPages;
    }

    getCurrentPage(offset: number, limit: number): number {
        return Math.floor(offset / limit) + 1;
    }

    getTotalPages(limit: number, size: number): number {
        return Math.ceil(Math.max(size, 1) / Math.max(limit, 1));
    }

    selectPage(page: number, event) {

        if (this.isValidPageNumber(page, this.totalPages)) {
            this.pageChange.emit(page);
        }
    }

}
