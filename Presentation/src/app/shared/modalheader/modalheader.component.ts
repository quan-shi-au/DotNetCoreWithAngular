import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ROUTES } from '../.././sidebar/sidebar.component';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';


@Component({
    moduleId: module.id,
    selector: 'modalheader-cmp',
    templateUrl: 'modalheader.component.html',
    styleUrls: ['modalheader.css']
})

export class ModalheaderComponent implements OnInit{

    @Input() public resetMargin: boolean;
    @Input() public caption: string;
    @Input() public title: string;
    @Output() closeModal: EventEmitter<number> = new EventEmitter<number>();

    constructor(
    ) {
    }

    ngOnInit() {
    }

    closeIt() {
        this.closeModal.emit();
    }
}
