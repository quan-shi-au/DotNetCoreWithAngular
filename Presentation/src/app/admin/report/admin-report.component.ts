import { Component, OnInit, AfterViewInit, HostListener, OnDestroy } from '@angular/core'
import { Router } from '@angular/router';
import { AdminReportService } from '../../core/services/adminreport.service';
import { HttpErrorResponse } from '@angular/common/http';

import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable';
import { ModalService } from '../../core/services/modal.service';
import { PageService } from '../../core/services/page.service';

import { TranslateService } from '@ngx-translate/core';

declare var $: any;
declare var swal: any;

@Component({
    selector: 'admin-report',
    templateUrl: 'admin-report.component.html',
    styleUrls: ['admin-report.css']
})
export class AdminReportComponent implements OnInit, OnDestroy {
    public Reports: any;
    public headerRow: string[] = ['ID', 'Report Count', 'Start Time UTC', 'End Time UTC', 'Start Time Local', 'End Time Local', 'Status'  ];
    public waitingForResponse: boolean = false;

    public isNameValid: boolean = false;
    public isLocationValid: boolean = false;
    public isFormValid: boolean = false;

    public localCount: number = 0;
    public localOffset: number = 0;
    public localLimit: number = 0;  // page size

    public localCaption = "Add Report";
    public localTitle = "Add a new Report";
    public resetMargin = false;

    private subscription: any;

    constructor(
        private router: Router,
        private adminReportService: AdminReportService,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private modalService: ModalService,
        private translate: TranslateService,
        private pageService: PageService,
        private utilityService: UtilityService
    ) {
    }

    ngOnDestroy() {

        this.subscription.unsubscribe();
    }

    ngOnInit() {

        this.getReports();

        this.localLimit = this.pageService.getPageSize();
        this.subscription = this.pageService.pageSizeSubject.subscribe(item => { this.localLimit = item; });

    }

    getReports() {

        this.adminReportService.getAdminReport()
            .subscribe(
            (data: ApiResult) => {
                if (data.c === glob.SuccessCode) {
                    this.Reports = data.d.map(item => {
                        if (item.startTimeUTC)
                            item.startTimeLocal = this.utilityService.convertUtcToLocalDateWithAuFormat(item.startTimeUTC);

                        if (item.endTimeUTC)
                            item.endTimeLocal = this.utilityService.convertUtcToLocalDateWithAuFormat(item.endTimeUTC);

                        return item;
                    });

                }

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to get Report list.');
            }
            );

    }

    generateReport() {

        this.adminReportService.generateAdminReport()
            .subscribe(
                (data: any) => {
                },
                (err: HttpErrorResponse) => {
                }
            );

        this.toastrService.info("Report generation has been triggered.");
    }

}