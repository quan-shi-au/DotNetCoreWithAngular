import {
    Component, OnInit, Renderer, ViewChild, ElementRef, Directive, Input, Output, EventEmitter, AfterViewInit, OnChanges,
    SimpleChanges, SimpleChange
} from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { SearchCriteria } from '../../core/models/searchCriteria';
import * as Chartist from 'chartist';
import { ReportService } from '../../core/services/report.service';
import { ApiResult } from '../../core/models/apiResult';
import * as glob from '../../core/variables/global.variable';
import { AlertService } from '../../core/services/alert.service'
import { ChartistData } from '../../core/models/chartistData';

import { UsageReport } from '../../core/models/usageReport';

@Component({
    moduleId: module.id,
    selector: 'piechart-cmp',
    templateUrl: 'piechart.component.html',
    styleUrls: ['piechart.css']
})

export class PiechartComponent implements OnInit, AfterViewInit, OnChanges{

    @Input() legends;
    @Input() title;
    @Input() chartPreferencesId: string;
    @Input() dataPreferences;

    @Input() isChartAvailable: boolean;
    @Input() public noChartMessage: string;

    public subscriptionId: number;

    private optionsPreferences = {
        height: '190px'
    };


    constructor(
        location: Location,
        private renderer: Renderer,
        private element: ElementRef,
        private reportService: ReportService,
        private alertService: AlertService,

    ) {
    }

    ngOnInit() {

    }

    ngAfterViewInit() {

        this.renderPieChart();

    }

    ngOnChanges(changes: SimpleChanges) {
        this.renderPieChart();
    }

    renderPieChart() {

        if (this.dataPreferences && this.dataPreferences.series && this.dataPreferences.series.length > 0)
            new Chartist.Pie('#' + this.chartPreferencesId, this.dataPreferences, this.optionsPreferences);
    }


}
