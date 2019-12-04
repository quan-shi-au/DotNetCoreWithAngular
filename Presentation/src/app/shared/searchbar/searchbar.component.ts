import { Component, OnInit, Renderer, ViewChild, ElementRef, Directive, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';

import { SearchCriteria } from '../../core/models/searchCriteria';
import { SeatFilter } from '../../core/models/seatFilter';

@Component({
    moduleId: module.id,
    selector: 'searchbar-cmp',
    templateUrl: 'searchbar.component.html',
    styleUrls: ['searchbar.css']
})

export class SearchbarComponent implements OnInit{

    @Output() searchEvent = new EventEmitter();

    public searchCriteria: SeatFilter = {};

    constructor(location:Location, private renderer : Renderer, private element : ElementRef) {
    }

    ngOnInit(){
    }

    search(searchForm) {
        this.searchEvent.next(this.searchCriteria);
    }

}
